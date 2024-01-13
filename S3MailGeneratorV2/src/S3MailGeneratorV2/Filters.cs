using System.Globalization;
using Fluid;
using Fluid.Ast;
using Fluid.Values;

namespace S3MailGeneratorV2;
public class Filters
{
    public static ValueTask<FluidValue> NumberToMonth(FluidValue input,
        FilterArguments arguments, TemplateContext context)
    {
        string cultureString = arguments.At(0).ToStringValue();
        decimal number = input.ToNumberValue();

        CultureInfo ci = CultureInfo.CurrentCulture; 

        if (!string.IsNullOrEmpty(cultureString)) {
            ci = CultureInfo.GetCultureInfo(cultureString);
        }

        string monthName = ci.DateTimeFormat.GetMonthName((int)number);

        return new StringValue(monthName);
    }

    public static ValueTask<FluidValue> NumberFormat(FluidValue input,
        FilterArguments arguments, TemplateContext context)
    {
        string pattern = arguments.At(0).ToStringValue();
        decimal number = input.ToNumberValue();

        string result = number.ToString(pattern, context.CultureInfo);

        return new StringValue(result);
    }
}