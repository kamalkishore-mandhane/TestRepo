namespace SafeShare.Converter.ConvertUtil
{
    internal enum EXIFENUM
    {
        JEI_Artist = 1000,
        JEI_DateTime,
        JEI_ImageDescription,
        JEI_Make,
        JEI_Model,
        JEI_Software,
        BodySerialNumber,
        CameraOwnerName,
        GPSData,
        UserComment
    }

    internal enum XMPDCENUM
    {
        Contributors = 2000,
        Creators,
        Source,
        Subject
    }

    internal enum XMPPDFENUM
    {
        Keywords = 3000,
        Producer
    }

    internal enum XMPPSENUM
    {
        AuthorsPosition = 4000,
        CaptionWriter,
        City,
        Country,
        Credit,
        DateCreated,
        Headline,
        History,
        Instructions,
        Source,
        State
    }

    internal enum XMPXBENUM
    {
        BaseUrl = 5000,
        CreateDate,
        CreatorTool,
        Label,
        MetadataDate,
        ModifyDate,
        Nickname
    }

    internal enum DOCUMENTENUM
    {
        Author = 6000,
        Category,
        Comments,
        CreatedDate,
        Company,
        HyperlinkBase,
        Keywords,
        Manager,
        ModifiedDate,
        Subject,
        Title,
        LastSavedBy,
        ContentStatus
    }

    internal enum METADATA_DISPLAY_NAME
    {
        ARTIST = 3010,
        AUTHORS,
        AUTHORS_POSITION,
        BASEURL,
        CAMERA_MAKE,
        CAMERA_MODEL,
        CAMERA_OWNER_NAME,
        CAMERA_SERIAL_NUMBER,
        CAPTION_WRITER,
        CATEGORY,
        CITY,
        COMMENTS,
        COMPANY_NAME,
        CONTRIBUTORS,
        COUNTRY,
        CREATED_DATE,
        CREDIT,
        DESCRIPTION,
        GPS_LOCATION,
        HEADLINE,
        HISTORY,
        HYPERLINK,
        INSTRUCTIONS,
        KEYWORDS,
        LABEL,
        MANAGER_NAME,
        METADATA_DATE,
        MODIFIED_DATE,
        NICKNAME,
        PRODUCER,
        PROGRAM_NAME,
        SOURCE,
        STATE,
        SUBJECT,
        TITLE,
        LASTSAVEDBY,
        CONTENTSTATUS
    };

    internal enum MedataNum
    {
        METADATA_COUNT = 37,
        METADATA_START_NUM = 3010
    }
}