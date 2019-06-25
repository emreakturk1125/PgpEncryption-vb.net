Imports System.IO
Imports Org.BouncyCastle.Bcpg
Imports Org.BouncyCastle.Bcpg.OpenPgp
Imports Org.BouncyCastle.Security
Imports Org.BouncyCastle.Utilities.IO

Public Class PGPEncryptDecrypt

Private Const BufferSize As Integer = &H10000  'hexadecimal kullanımı

#Region "Şifreleme"

    ''' <summary>
    '''  ArmoredOutputStream, Base64'e benzer bir kodlama kullanır, 
    '''  böylece ikili yazdırılamaz baytlar metin dostu bir şeye dönüştürülür.
    '''  Verileri e-postayla göndermek, bir siteye veya başka bir metin ortamına göndermek istiyorsanız bunu yaparsınız.
    '''  'Güvenlik açısından bir fark yaratmaz. 
    '''  Çıktı olarak Outputstream ve ArmoredOutputStream farklı çıktı verir
    ''' </summary> 
    ''' <param name="armor"></param>
    Public Shared Sub EncryptAndSign(ByVal inputFile As String, ByVal outputFile As String, ByVal publicKeyFile As String, ByVal privateKeyFile As String, ByVal passPhrase As String, ByVal armor As Boolean)

        Dim encryptionKeys As PgpEncryptionKeys = New PgpEncryptionKeys(publicKeyFile, privateKeyFile, passPhrase)
        If Not File.Exists(inputFile) Then
            Throw New FileNotFoundException(String.Format("Şifrelenecek dosya [{0}] bulunamadı.", inputFile))
        End If
        If Not File.Exists(publicKeyFile) Then
            Throw New FileNotFoundException(String.Format("Public Key dosyası [{0}] bulunamadı.", publicKeyFile))
        End If
        If Not File.Exists(privateKeyFile) Then
            Throw New FileNotFoundException(String.Format("Private Key dosyası [{0}] bulunamadı.", privateKeyFile))
        End If
        If String.IsNullOrEmpty(passPhrase) Then
            Throw New ArgumentNullException("Geçersiz şifre.")
        End If
        If encryptionKeys Is Nothing Then
            Throw New ArgumentNullException("Şifreleme için anahtarlar bulunamadı.")
        End If

        Using outputStream As Stream = File.Create(outputFile)

            If armor Then
                Using armoredOutputStream As ArmoredOutputStream = New ArmoredOutputStream(outputStream)
                    OutputEncrypted(inputFile, armoredOutputStream, encryptionKeys)
                End Using
            Else
                OutputEncrypted(inputFile, outputStream, encryptionKeys)
            End If

        End Using

    End Sub

    Private Shared Sub OutputEncrypted(ByVal inputFile As String, ByVal outputStream As Stream, ByVal encryptionKeys As PgpEncryptionKeys)

        Using encryptedOut As Stream = ChainEncryptedOut(outputStream, encryptionKeys)
            Dim unencryptedFileInfo As FileInfo = New FileInfo(inputFile)

            Using compressedOut As Stream = ChainCompressedOut(encryptedOut)
                Dim signatureGenerator As PgpSignatureGenerator = InitSignatureGenerator(compressedOut, encryptionKeys)

                Using literalOut As Stream = ChainLiteralOut(compressedOut, unencryptedFileInfo)

                    Using inputFileStream As FileStream = unencryptedFileInfo.OpenRead()
                        WriteOutputAndSign(compressedOut, literalOut, inputFileStream, signatureGenerator)
                        inputFileStream.Close()
                    End Using
                End Using
            End Using
        End Using

    End Sub


    Private Shared Sub WriteOutputAndSign(ByVal compressedOut As Stream, ByVal literalOut As Stream, ByVal inputFile As FileStream, ByVal signatureGenerator As PgpSignatureGenerator)
        Dim length As Integer = 0
        Dim buf As Byte() = New Byte(BufferSize - 1) {}

        While (AssignData.Assign(length, inputFile.Read(buf, 0, buf.Length))) > 0
            literalOut.Write(buf, 0, length)
            signatureGenerator.Update(buf, 0, length)
        End While

        signatureGenerator.Generate().Encode(compressedOut)
    End Sub


    Private Shared Function ChainEncryptedOut(ByVal outputStream As Stream, ByVal m_encryptionKeys As PgpEncryptionKeys) As Stream

        Dim encryptedDataGenerator As PgpEncryptedDataGenerator
        encryptedDataGenerator = New PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.TripleDes, New SecureRandom())
        encryptedDataGenerator.AddMethod(m_encryptionKeys.PublicKey)
        Return encryptedDataGenerator.Open(outputStream, New Byte(BufferSize - 1) {})

    End Function

    Private Shared Function ChainCompressedOut(ByVal encryptedOut As Stream) As Stream

        Dim compressedDataGenerator As PgpCompressedDataGenerator = New PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip)
        Return compressedDataGenerator.Open(encryptedOut)

    End Function

    Private Shared Function ChainLiteralOut(ByVal compressedOut As Stream, ByVal file As FileInfo) As Stream

        Dim pgpLiteralDataGenerator As PgpLiteralDataGenerator = New PgpLiteralDataGenerator()
        Return pgpLiteralDataGenerator.Open(compressedOut, PgpLiteralData.Binary, file)

    End Function
    Private Shared Function InitSignatureGenerator(ByVal compressedOut As Stream, ByVal m_encryptionKeys As PgpEncryptionKeys) As PgpSignatureGenerator

        Const IsCritical As Boolean = False
        Const IsNested As Boolean = False
        Dim tag As PublicKeyAlgorithmTag = m_encryptionKeys.SecretKey.PublicKey.Algorithm
        Dim pgpSignatureGenerator As PgpSignatureGenerator = New PgpSignatureGenerator(tag, HashAlgorithmTag.Sha1)
        pgpSignatureGenerator.InitSign(PgpSignature.BinaryDocument, m_encryptionKeys.PrivateKey)

        For Each userId As String In m_encryptionKeys.SecretKey.PublicKey.GetUserIds()
            Dim subPacketGenerator As PgpSignatureSubpacketGenerator = New PgpSignatureSubpacketGenerator()
            subPacketGenerator.SetSignerUserId(IsCritical, userId)
            pgpSignatureGenerator.SetHashedSubpackets(subPacketGenerator.Generate())
            Exit For
        Next

        pgpSignatureGenerator.GenerateOnePassVersion(IsNested).Encode(compressedOut)
        Return pgpSignatureGenerator

    End Function

#End Region

#Region "Şifre çözme"

    Public Shared Sub Decrypt(ByVal inputfile As String, ByVal privateKeyFile As String, ByVal passPhrase As String, ByVal outputFile As String)

        If Not File.Exists(inputfile) Then
            Throw New FileNotFoundException(String.Format("Şifreli dosya [{0}] bulunamadı.", inputfile))
        End If
        If Not File.Exists(privateKeyFile) Then
            Throw New FileNotFoundException(String.Format("Private Key dosyası [{0}] bulunamadı.", privateKeyFile))
        End If
        If String.IsNullOrEmpty(outputFile) Then
            Throw New ArgumentNullException("Geçersiz dosya yolu.")
        End If

        Using inputStream As Stream = File.OpenRead(inputfile)

            Using keyIn As Stream = File.OpenRead(privateKeyFile)
                Decrypt(inputStream, keyIn, passPhrase, outputFile)
            End Using
        End Using

    End Sub

    Public Shared Sub Decrypt(ByVal inputStream As Stream, ByVal privateKeyStream As Stream, ByVal passPhrase As String, ByVal outputFile As String)

        Try
            Dim pgpF As PgpObjectFactory = Nothing
            Dim enc As PgpEncryptedDataList = Nothing
            Dim o As PgpObject = Nothing
            Dim sKey As PgpPrivateKey = Nothing
            Dim pbe As PgpPublicKeyEncryptedData = Nothing
            Dim pgpSec As PgpSecretKeyRingBundle = Nothing
            pgpF = New PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream))
            pgpSec = New PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream))
            If pgpF IsNot Nothing Then
                o = pgpF.NextPgpObject()
            End If

            If TypeOf o Is PgpEncryptedDataList Then
                enc = CType(o, PgpEncryptedDataList)
            Else
                enc = CType(pgpF.NextPgpObject(), PgpEncryptedDataList)
            End If

            For Each pked As PgpPublicKeyEncryptedData In enc.GetEncryptedDataObjects()
                sKey = FindSecretKey(pgpSec, pked.KeyId, passPhrase.ToCharArray())

                If sKey IsNot Nothing Then
                    pbe = pked
                    Exit For
                End If
            Next

            If sKey Is Nothing Then
                Throw New ArgumentException("Secret key for message not found.")
            End If

            Dim plainFact As PgpObjectFactory = Nothing

            Using clear As Stream = pbe.GetDataStream(sKey)
                plainFact = New PgpObjectFactory(clear)
            End Using

            Dim message As PgpObject = plainFact.NextPgpObject()

            If TypeOf message Is PgpCompressedData Then
                Dim cData As PgpCompressedData = CType(message, PgpCompressedData)
                Dim [of] As PgpObjectFactory = Nothing

                Using compDataIn As Stream = cData.GetDataStream()
                    [of] = New PgpObjectFactory(compDataIn)
                End Using

                message = [of].NextPgpObject()

                If TypeOf message Is PgpOnePassSignatureList Then
                    message = [of].NextPgpObject()
                    Dim Ld As PgpLiteralData = Nothing
                    Ld = CType(message, PgpLiteralData)

                    Using output As Stream = File.Create(outputFile)
                        Dim unc As Stream = Ld.GetInputStream()
                        Streams.PipeAll(unc, output)
                    End Using
                Else
                    Dim Ld As PgpLiteralData = Nothing
                    Ld = CType(message, PgpLiteralData)

                    Using output As Stream = File.Create(outputFile)
                        Dim unc As Stream = Ld.GetInputStream()
                        Streams.PipeAll(unc, output)
                    End Using
                End If
            ElseIf TypeOf message Is PgpLiteralData Then
                Dim ld As PgpLiteralData = CType(message, PgpLiteralData)
                Dim outFileName As String = ld.FileName

                Using fOut As Stream = File.Create(outputFile)
                    Dim unc As Stream = ld.GetInputStream()
                    Streams.PipeAll(unc, fOut)
                End Using
            ElseIf TypeOf message Is PgpOnePassSignatureList Then
                Throw New PgpException("Encrypted message contains a signed message - not literal data.")
            Else
                Throw New PgpException("Message is not a simple encrypted file - type unknown.")
            End If

        Catch ex As PgpException
            Throw
        End Try

    End Sub

    Private Shared Function FindSecretKey(ByVal pgpSec As PgpSecretKeyRingBundle, ByVal keyId As Long, ByVal pass As Char()) As PgpPrivateKey

        Dim pgpSecKey As PgpSecretKey = pgpSec.GetSecretKey(keyId)
        If pgpSecKey Is Nothing Then
            Return Nothing
        End If
        Return pgpSecKey.ExtractPrivateKey(pass)

    End Function

#End Region

End Class

Public Class AssignData
    Shared Function Assign(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function

End Class

