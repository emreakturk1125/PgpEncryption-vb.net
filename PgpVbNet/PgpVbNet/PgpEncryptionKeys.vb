Imports System.IO
Imports Org.BouncyCastle.Bcpg.OpenPgp
Imports Org.BouncyCastle.Utilities.IO

Public Class PgpEncryptionKeys

    Public Property PublicKey As PgpPublicKey
    Public Property PrivateKey As PgpPrivateKey
    Public Property SecretKey As PgpSecretKey

    Public Sub New(ByVal publicKeyPath As String, ByVal privateKeyPath As String, ByVal passPhrase As String)

        If Not File.Exists(publicKeyPath) Then
            Throw New ArgumentException("Public key file not found", "publicKeyPath")
        End If
        If Not File.Exists(privateKeyPath) Then
            Throw New ArgumentException("Private key file not found", "privateKeyPath")
        End If
        If String.IsNullOrEmpty(passPhrase) Then
            Throw New ArgumentException("passPhrase is null or empty.", "passPhrase")
        End If

        PublicKey = ReadPublicKey(publicKeyPath)
        SecretKey = ReadSecretKey(privateKeyPath)
        PrivateKey = ReadPrivateKey(passPhrase)

    End Sub

     Private Function ReadSecretKey(ByVal privateKeyPath As String) As PgpSecretKey
        Using keyIn As Stream = File.OpenRead(privateKeyPath)

            Using inputStream As Stream = PgpUtilities.GetDecoderStream(keyIn)
                Dim secretKeyRingBundle As PgpSecretKeyRingBundle = New PgpSecretKeyRingBundle(inputStream)
                Dim foundKey As PgpSecretKey = GetFirstSecretKey(secretKeyRingBundle)
                If foundKey IsNot Nothing Then
                    Return foundKey
                End If
            End Using
        End Using

        Throw New ArgumentException("Can't find signing key in key ring.")
    End Function

      Private Function GetFirstSecretKey(ByVal secretKeyRingBundle As PgpSecretKeyRingBundle) As PgpSecretKey

        For Each kRing As PgpSecretKeyRing In secretKeyRingBundle.GetKeyRings()
            Dim key As PgpSecretKey = kRing.GetSecretKeys().Cast(Of PgpSecretKey)().Where(Function(k) k.IsSigningKey).FirstOrDefault()
            If key IsNot Nothing Then
                Return key
            End If
        Next

        Return Nothing
    End Function


    Private Function ReadPublicKey(ByVal publicKeyPath As String) As PgpPublicKey

        Using keyIn As Stream = File.OpenRead(publicKeyPath)

            Using inputStream As Stream = PgpUtilities.GetDecoderStream(keyIn)
                Dim publicKeyRingBundle As PgpPublicKeyRingBundle = New PgpPublicKeyRingBundle(inputStream)
                Dim foundKey As PgpPublicKey = GetFirstPublicKey(publicKeyRingBundle)
                If foundKey IsNot Nothing Then Return foundKey
            End Using

        End Using
        Throw New ArgumentException("Şifreleme keyleri bulunamadı")

    End Function

     Private Function GetFirstPublicKey(ByVal publicKeyRingBundle As PgpPublicKeyRingBundle) As PgpPublicKey

        For Each kRing As PgpPublicKeyRing In publicKeyRingBundle.GetKeyRings()
            Dim key As PgpPublicKey = kRing.GetPublicKeys().Cast(Of PgpPublicKey)().Where(Function(k) k.IsEncryptionKey).FirstOrDefault()
            If key IsNot Nothing Then
                Return key
            End If
        Next
        Return Nothing

    End Function

    Private Function ReadPrivateKey(ByVal passPhrase As String) As PgpPrivateKey

        Dim privateKey As PgpPrivateKey = SecretKey.ExtractPrivateKey(passPhrase.ToCharArray())
        If privateKey IsNot Nothing Then
            Return privateKey
        End If
        Throw New ArgumentException("No private key found in secret key.")

    End Function

  
End Class
