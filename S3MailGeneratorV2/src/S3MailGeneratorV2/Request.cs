namespace S3MailGeneratorV2;

public class Request
{
    public string Region {get; set;}
    public string Bucketname {get; set;}
    public string Source {get; set;}

    public string ResultPrefix {get; set;}
}