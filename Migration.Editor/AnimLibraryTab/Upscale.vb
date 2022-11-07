Imports System.Drawing.Imaging
Imports System.IO
Imports System
Imports System.Windows.Controls

Namespace Migration.Editor
    ''' <summary>
    ''' Interaction logic for AnimLibraryTab.xaml
    ''' </summary>
    Partial Public Class AnimLibraryTab
        Inherits UserControl

        ''' <summary>
        ''' Produces an image which is upscaled by a factor of 4. This requires the processor "hq4x.exe"
        ''' to be in the application directory!
        ''' </summary>
        ''' <param name="inSource"></param>
        ''' <returns></returns>
        Private Shared Function Upscale(ByVal inSource As Bitmap) As Bitmap
            If Not (File.Exists("hq4x.exe")) Then
                Throw New NotSupportedException("The required processor ""hq4x.exe"" is missing.")
            End If

            Dim srcPath As String = System.IO.Path.GetTempFileName()
            Dim dstPath As String = System.IO.Path.GetTempFileName()

            Try
                inSource.Save(srcPath, ImageFormat.Bmp)

                Dim hq4x = System.Diagnostics.Process.Start("hq4x.exe", """" & srcPath & """ """ & dstPath & """")

                If Not (hq4x.WaitForExit(10000)) Then
                    hq4x.Kill()

                    Throw New ApplicationException("Scaling could not be completed, since processor ""hq4x.exe"" did not complete in reasonable time.")
                End If

                Try
                    hq4x.Kill()
                Catch
                End Try

                Dim data As New MemoryStream(File.ReadAllBytes(dstPath))

                Return CType(Bitmap.FromStream(data), Bitmap)
            Finally
                Try
                    File.Delete(srcPath)
                Catch
                End Try
                Try
                    File.Delete(dstPath)
                Catch
                End Try
            End Try
            'INSTANT C# NOTE: Inserted the following 'return' since all code paths must return a value in C#:
            Return Nothing
        End Function
    End Class
End Namespace
