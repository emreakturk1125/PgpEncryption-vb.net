Imports System.IO
Imports System.Text
Imports DidiSoft.Pgp
Imports Microsoft.Office.Interop.Excel

Module Module1

    Sub Main()

        Console.WriteLine("Pgp şifreleme devam etmek istiyormusun(e/h) ?")
        Dim cvp1 As String = Convert.ToString(Console.ReadLine)
        If cvp1 = "e" Then
            Encrypt()
            Console.WriteLine("Şifrelendi")
        End If

        Console.WriteLine("Pgp şifreli dosyayı çözmek istiyormusun(e/h) ?")
        Dim cvp2 As String = Convert.ToString(Console.ReadLine)
        If cvp2 = "e" Then
            Decrypt()
            Console.WriteLine("Şifre çözüldü")
        End If

        Console.WriteLine(".xlx  ve  cvs dosyalarına aktarmak istiyormusun?")

        Dim cvp4 As String = Convert.ToString(Console.ReadLine)
        If cvp4 = "e" Then
             Dim path As String = "C:\Users\emre.akturk\Desktop\bilin\ŞifrelemeKlasör\DecryptedOutput.txt"
             Dim table As System.Data.DataTable = ReadFile(path)
             Excel_FromDataTable(table)
             Console.WriteLine(".xlx  ve .csv formarlarına aktarıldı")
        End If

        Console.ReadKey()

    End Sub



    Public Sub Encrypt()
        Dim pgp As New PGPLib()

        Dim asciiArmor As Boolean = True
        Dim withIntegrityCheck As Boolean = False

        Dim Input As String = "C:\Users\emre.akturk\Desktop\bilin\ŞifrelemeKlasör\inputFile.txt"
        Dim publicKey As String = "C:\Users\emre.akturk\Desktop\bilin\bilin_public_key.asc"
        Dim output As String = "C:\Users\emre.akturk\Desktop\bilin\ŞifrelemeKlasör\EncryptedOutput.pgp"

        pgp.EncryptFile(Input, publicKey, output, asciiArmor, withIntegrityCheck)

    End Sub

    Public Sub Decrypt()

        Dim pgp As New PGPLib()

        Dim Input As String = "C:\Users\emre.akturk\Desktop\bilin\ŞifrelemeKlasör\EncryptedOutput.pgp"
        Dim privateKey As String = "C:\Users\emre.akturk\Desktop\bilin\bilin_private_key.asc"
        Dim output As String = "C:\Users\emre.akturk\Desktop\bilin\ŞifrelemeKlasör\DecryptedOutput.txt"

        Dim originalFileName As String
        originalFileName = pgp.DecryptFile(Input, privateKey, "bilinyazilim34", output)
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

    Private Function ReadFile(ByVal path As String) As System.Data.DataTable
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
