using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Fluid;
using MimeKit;
using Newtonsoft.Json.Linq;

namespace S3MailGeneratorV2;

public class Generator
{
    private IAmazonS3 _s3Client;

    private RegionEndpoint _region = null;

    public Generator(string Region)
    {
        // Set the AWS region where your S3 bucket is located
        _region = RegionEndpoint.GetBySystemName(Region);

        // Create an S3 client
        _s3Client = new AmazonS3Client(_region);
    }

    /// <summary>
    /// Takes a source json file that describes the mails
    /// that needs to be generated.
    /// </summary>
    /// <param name="Source"></param>
    public void Generate(string Bucketname, string Source)
    {
        // Parse json file
        // Read JSON file content
        string content = GetFileContentFromS3(Bucketname, Source).Result;

        // Parse JSON
        JArray json = JArray.Parse(content);

        foreach (var m in json)
        {
            var mailElement = m["Mail"];

            // Make eml file in S3
            var message = new MimeMessage();

            string fromAddress = (string)mailElement["From"];
            string fromAddressName = (string)mailElement["FromName"];

            if (fromAddressName == null)
            {
                fromAddressName = fromAddress;
            }

            message.From.Add(new MailboxAddress(fromAddressName, fromAddressName));

            string toAddress = (string)mailElement["To"];
            string toAddressName = (string)mailElement["ToName"];

            if (toAddressName == null)
            {
                toAddressName = toAddress;
            }

            message.To.Add(new MailboxAddress(toAddress, toAddress));

            string subject = (string)mailElement["Subject"];

            message.Subject = subject;

            // Content
            var c = m["Content"];

            var parser = new FluidParser();

            // Get template
            string templatePath = (string)mailElement["Template"];

            string templateText = GetFileContentFromS3(Bucketname, templatePath).Result;

            if (parser.TryParse(templateText, out var template, out var error))
            {
                var templateContext = MakeTemplateContext();

                // Add content to  template
                templateContext.SetValue("Content", c);

                string resultString = template.Render(templateContext);

                message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = resultString
                };

                // Write result file
                string ResultFile = (string)mailElement["Result"];

                MemoryStream ms = new MemoryStream();
                message.WriteTo(ms);

                SaveFile(_s3Client, Bucketname, ResultFile,
                    ms, "application/octet-stream");
            }
        }
    }

    public void SaveFile(IAmazonS3 _s3Client, string Bucketname,
        string S3Path, Stream Stream, string ContentType)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = Bucketname,
            Key = S3Path,
            ContentType = ContentType,
            InputStream = Stream
        };

        _s3Client.PutObjectAsync(putRequest).Wait();
    }

    public TemplateContext MakeTemplateContext()
    {
        var options = new TemplateOptions();
        options.MemberAccessStrategy = UnsafeMemberAccessStrategy.Instance;

        TemplateContext templateContext = new TemplateContext(options);

        // When a property of a JsonObjectvalue is accessed, try to look into its properties
        //        options.MemberAccessStrategy.Register<JsonObject, object?>((source, name) => source[name]);

        //        options.ValueConverters.Add(x => x is JsonObject o ? new ObjectValue(o) : null);
        //        options.ValueConverters.Add(x => x is JsonValue v ? v.GetValue<object>() : null);

        return templateContext;
    }


    private async Task<string> GetFileContentFromS3(string bucketName, string key)
    {
        try
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using (GetObjectResponse response = await _s3Client.GetObjectAsync(request))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
            {
                return await reader.ReadToEndAsync();
            }
        }
        catch (AmazonS3Exception e)
        {
            // Handle S3 exception
            return $"Error getting template: {e.Message}";
        }
    }
}