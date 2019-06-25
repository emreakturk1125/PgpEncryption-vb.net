Imports System.IO
Imports System.Text
Imports Microsoft.Office.Interop.Excel

Module Module1

    Sub Main()

        Dim fileLocation As String = "C:\Users\emre.akturk\Desktop\bilin\ŞifrelemeKlasör"

        If Not Directory.Exists(fileLocation) Then
            Directory.CreateDirectory(fileLocation)
        End If

        Dim writer As StreamWriter
        Dim ostrm As FileStream
        Dim fileLocationInput As String = "C:\Users\emre.akturk\Desktop\bilin\ŞifrelemeKlasör\inputFile.txt"
        Dim fileLocationEncrytedInput As String = "C:\Users\emre.akturk\Desktop\bilin\ŞifrelemeKlasör\EncryptedInputFile.txt"
        Dim fileLocationDecryptedOutput As String = "C:\Users\emre.akturk\Desktop\bilin\ŞifrelemeKlasör\DecryptedOutputFile.txt"

        If Not File.Exists(fileLocationInput) Then

            ostrm = File.Create(fileLocationInput)
            ostrm = File.Create(fileLocationInput)
            ostrm = File.Create(fileLocationInput)

        End If

        Console.WriteLine("Şifrelemek istediğiniz metni giriniz : ")
        Dim yazı As String = Console.ReadLine()
        Console.WriteLine("Done")
        Console.WriteLine("-----------------------------------------------------")
        Console.WriteLine("Şifrelemek istiyormusun ?(E/H)")
        Dim cevap As Char = Convert.ToChar(Console.ReadLine())

        If cevap = "E" OrElse cevap = "e" Then
            PGPEncryptDecrypt.EncryptAndSign(fileLocationInput, fileLocationEncrytedInput, "C:\Users\emre.akturk\Desktop\kalın\kalin_public_key.asc", "C:\Users\emre.akturk\Desktop\bilin\bilin_private_key.asc", "bilinyazilim34", True)
            Console.WriteLine("Encryted..")
            Dim line As String

            Try
                Dim sr As StreamReader = New StreamReader(fileLocationEncrytedInput)
                line = sr.ReadLine()

                While line IsNot Nothing
                    Console.WriteLine(line)
                    line = sr.ReadLine()
                End While

                sr.Close()
            Catch e As Exception
                Console.WriteLine("Exception: " & e.Message)
            End Try
        End If

        Console.WriteLine("-----------------------------------------------------")
        Console.WriteLine("Şifreyi çözmek  istiyormusun ?(E/H)")
        Dim cevap2 As Char = Convert.ToChar(Console.ReadLine())

        If cevap = "E" OrElse cevap = "e" Then
            PGPEncryptDecrypt.Decrypt(fileLocationEncrytedInput, "C:\Users\emre.akturk\Desktop\kalın\kalin_private_key.asc", "kalinyazilim34", fileLocationDecryptedOutput)
            Console.WriteLine("Decrypted..")
            Dim line As String

            Try
                Dim sr As StreamReader = New StreamReader(fileLocationDecryptedOutput)
                line = sr.ReadLine()

                While line IsNot Nothing
                    Console.WriteLine(line)
                    line = sr.ReadLine()
                End While

                sr.Close()
            Catch e As Exception
                Console.WriteLine("Exception: " & e.Message)
            End Try
        End If

        Console.WriteLine("-----------------------------------------------------")
        Console.WriteLine("Verileri  .xlx  ve   .csv formarlarına aktarmak istiyormusun(e/h)")

        Dim cvp As String =  Convert.ToString(Console.ReadLine())

        If cvp = "e" Then
            Dim path As String = "C:\Users\emre.akturk\Desktop\bilin\ŞifrelemeKlasör\inputFile.txt"
            Dim table As System.Data.DataTable = ReadFile(path)
            Excel_FromDataTable(table)
             Console.WriteLine(".xlx  ve   .csv formarlarına aktarıldı")
        End If


        Console.ReadKey()

    End Sub

    Private Sub Excel_FromDataTable(ByVal dt As System.Data.DataTable)
        Dim excel As Application = New Application()
        Dim workbook As Workbook = excel.Application.Workbooks.Add(True)
        Dim iCol As Integer = 0

        For Each c As DataColumn In dt.Columns
            iCol += 1
            excel.Cells(1, iCol) = c.ColumnName
        Next

        Dim iRow As Integer = 0

        For Each r As DataRow In dt.Rows
            iRow += 1
            iCol = 0

            For Each c As DataColumn In dt.Columns
                iCol += 1
                excel.Cells(iRow + 1, iCol) = r(c.ColumnName)
            Next
        Next

        Dim myWorkSheet As Worksheet = CType(workbook.Worksheets.Item(1), Worksheet)
        Dim range As Range = CType(myWorkSheet.Application.Rows(1, Type.Missing), Range)
        range.[Select]()
        range.Delete(XlDirection.xlUp)
        Dim missing As Object = System.Reflection.Missing.Value
        workbook.SaveAs("C:\Users\emre.akturk\Desktop\bilin\ŞifrelemeKlasör\inputFile(" & Guid.NewGuid().ToString & ").xls", XlFileFormat.xlXMLSpreadsheet, missing, missing, False, False, XlSaveAsAccessMode.xlNoChange, missing, missing, missing, missing, missing)
        excel.Visible = True
        Dim worksheet As Worksheet = CType(excel.ActiveSheet, Worksheet)
        CType(worksheet, _Worksheet).Activate()
        CType(excel, _Application).Quit()
         Dim tbl As System.Data.DataTable = dt
        Dim filePath As String = "C:\Users\emre.akturk\Desktop\bilin\ŞifrelemeKlasör\test.csv"
        Dim delimiter As String = ","
        Dim sb As StringBuilder = New StringBuilder()
        Dim CsvRow As List(Of String) = New List(Of String)()

        For Each r As DataRow In dt.Rows
            CsvRow.Clear()

            For Each c As DataColumn In dt.Columns
                CsvRow.Add(r(c).ToString())
            Next

            sb.AppendLine(String.Join(delimiter, CsvRow))
        Next

        File.WriteAllText(filePath, sb.ToString())
    End Sub

     Private  Function ReadFile(ByVal path As String) As System.Data.DataTable
        Dim table As System.Data.DataTable = New System.Data.DataTable("dataFromFile")
        table.Columns.AddRange(New DataColumn() {New DataColumn("col1", GetType(String))})

        Using sr As StreamReader = New StreamReader(path)
            Dim line As String = ""
            While (AssignValue.Assign(line, sr.ReadLine())) IsNot Nothing
                Dim tempRw As DataRow = table.NewRow()
                tempRw("col1") = line
                table.Rows.Add(tempRw)
            End While
        End Using

        Return table
    End Function

    Private Class AssignValue
        Shared Function Assign(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class

End Module
