using Amazon.Lambda.Core;
using Org.BouncyCastle.Ocsp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace S3MailGeneratorV2;

public class Function
{    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public string FunctionHandler(Request Request, ILambdaContext context)
    {
        Generator generator = new Generator(Request.Region);

        generator.Generate(Request.Bucketname, Request.Source, Request.ResultPrefix);

        return "OK";
    }
}
