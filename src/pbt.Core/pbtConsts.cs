using pbt.Debugging;
using System.ComponentModel;

namespace pbt
{
    public class pbtConsts
    {
        public const string LocalizationSourceName = "pbt";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = false;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "eb79c0865c6843018545d7de06b355e6";

    }

  
}
