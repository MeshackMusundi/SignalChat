Imports System.Globalization

Public Class ByteBitmapSourceConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If value Is Nothing Then Return Nothing
        Dim img = CType(value, Byte())
        Dim converter As New ImageSourceConverter()
        Dim bmpSrc = CType(converter.ConvertFrom(img), BitmapSource)
        Return bmpSrc
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Binding.DoNothing
    End Function
End Class
