Imports AutoMapper
Imports System.Globalization
Public Class MappingProfile
    Inherits Profile

    Public Sub New()
        CreateMap(Of BlockDto, Block)() _
            .ForMember(Function(dest) dest.BlockNumber, Sub(opt) opt.MapFrom(Function(src) Long.Parse(src.Number.Substring(2), Globalization.NumberStyles.HexNumber))) _
            .ForMember(Function(dest) dest.GasLimit, Sub(opt) opt.MapFrom(Function(src) ConvertHexToDecimal(src.GasLimit))) _
            .ForMember(Function(dest) dest.GasUsed, Sub(opt) opt.MapFrom(Function(src) ConvertHexToDecimal(src.GasUsed))) _
            .ForMember(Function(dest) dest.BlockReward, Sub(opt) opt.Ignore()) ' Assuming block reward needs special handling

        CreateMap(Of TransactionDto, Transaction)() _
            .ForMember(Function(dest) dest.Value, Sub(opt) opt.MapFrom(Function(src) ConvertHexToDecimal(src.Value))) _
            .ForMember(Function(dest) dest.Gas, Sub(opt) opt.MapFrom(Function(src) ConvertHexToDecimal(src.Gas))) _
            .ForMember(Function(dest) dest.GasPrice, Sub(opt) opt.MapFrom(Function(src) ConvertHexToDecimal(src.GasPrice))) _
            .ForMember(Function(dest) dest.TransactionIndex, Sub(opt) opt.MapFrom(Function(src) Integer.Parse(src.TransactionIndex.Substring(2), Globalization.NumberStyles.HexNumber)))
    End Sub

    Private Function ConvertHexToDecimal(hexValue As String) As Decimal
        Return Convert.ToDecimal(Long.Parse(hexValue.Substring(2), Globalization.NumberStyles.HexNumber))
    End Function
End Class