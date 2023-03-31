using System.ComponentModel;

namespace Hci.WebsiteDolly.Core.Domain
{
    public enum ImportType
    {
        [Description("images")]
        Images,
        [Description("scripts")]
        JavaScript,
        [Description("stylesheets")]
        StyleSheets,
        [Description("html objects")]
        Html,
        [Description("anchors")]
        Anchors,
        [Description("video")]
        Video,
        [Description("other")]
        Miscellaneous
    }

    public enum ProcessorResultStatus
    {
        NotSet,
        Success,
        Fail,
        Exception
    }
}
