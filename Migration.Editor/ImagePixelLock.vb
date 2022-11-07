Imports System.Drawing.Imaging

Namespace Migration.Editor
	Public Class ImagePixelLock
		Inherits System.Runtime.ConstrainedExecution.CriticalFinalizerObject
		Implements IDisposable

		Private Shared buffer(1023) As Byte
		Private Shared tmpBuffer(1023) As Byte
		Private bitmap As Bitmap
		Private data As BitmapData

		Private privateIsCopy As Boolean
		Public Property IsCopy() As Boolean
			Get
				Return privateIsCopy
			End Get
			Private Set(ByVal value As Boolean)
				privateIsCopy = value
			End Set
		End Property

		Private privateChecksum As Int64
		Public Property Checksum() As Int64
			Get
				Return privateChecksum
			End Get
			Private Set(ByVal value As Int64)
				privateChecksum = value
			End Set
		End Property

		Private privatePixels As Integer
		Public Property Pixels() As Integer
			Get
				Return privatePixels
			End Get
			Private Set(ByVal value As Integer)
				privatePixels = value
			End Set
		End Property

		Public ReadOnly Property Width() As Integer
			Get
				Return bitmap.Width
			End Get
		End Property
		Public ReadOnly Property Height() As Integer
			Get
				Return bitmap.Height
			End Get
		End Property

		Public Sub New(ByVal inSource As Bitmap)
			Me.New(inSource, New System.Drawing.Rectangle(0, 0, inSource.Width, inSource.Height), False)
		End Sub

		Public Sub New(ByVal inSource As Bitmap, ByVal inCreateCopy As Boolean)
			Me.New(inSource, New System.Drawing.Rectangle(0, 0, inSource.Width, inSource.Height), inCreateCopy)
		End Sub

		Public Sub New(ByVal inSource As Bitmap, ByVal inLockRegion As System.Drawing.Rectangle)
			Me.New(inSource, inLockRegion, False)
		End Sub

		Public Sub New(ByVal inSource As Bitmap, ByVal inLockRegion As System.Drawing.Rectangle, ByVal inCreateCopy As Boolean)
			If inSource.PixelFormat <> System.Drawing.Imaging.PixelFormat.Format32bppArgb Then
				Throw New ArgumentException("Given bitmap has an unsupported pixel format.")
			End If

			IsCopy = inCreateCopy

			If inCreateCopy Then
				bitmap = CType(inSource.Clone(), Bitmap)
			Else
				bitmap = inSource
			End If

			data = bitmap.LockBits(inLockRegion, ImageLockMode.ReadWrite, inSource.PixelFormat)
			Pixels = Convert.ToInt32(data.Scan0)

			' compute checksum from pixeldata
			Dim md5 As System.Security.Cryptography.MD5 = System.Security.Cryptography.MD5.Create()
			Dim ptr As Integer = Convert.ToInt32(data.Scan0)

			Dim i As Integer = 0
			Dim byteCount As Integer = Width * Height * 4

			Do While i < byteCount
				Dim count As Integer = Math.Min(buffer.Length, byteCount - i)

				System.Runtime.InteropServices.Marshal.Copy(New IntPtr(ptr), buffer, 0, count)
				md5.TransformBlock(buffer, 0, count, tmpBuffer, 0)

				ptr += count \ 4
				i += buffer.Length
			Loop

			md5.TransformFinalBlock(New Byte(){}, 0, 0)

'INSTANT VB NOTE: The variable checksum was renamed since Visual Basic does not handle local variables named the same as class members well:
			Dim checksum_Renamed() As Byte = md5.Hash

			For i = 0 To 7
				Me.Checksum = (Me.Checksum Or (checksum_Renamed(i) << (i * 8)))
			Next i

		End Sub

		Public Overrides Function GetHashCode() As Integer
			Return Convert.ToInt32(Checksum)
		End Function

		Protected Overrides Sub Finalize()
			Dispose()
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			Try
				If (data IsNot Nothing) AndAlso (bitmap IsNot Nothing) Then
					bitmap.UnlockBits(data)
				End If

				If IsCopy AndAlso (bitmap IsNot Nothing) Then
					bitmap.Dispose()
				End If
			Catch
				' bitmap might be already disposed even if we got a valid pixellock
			End Try

			data = Nothing
			bitmap = Nothing
			Pixels = 0
		End Sub
	End Class
End Namespace
