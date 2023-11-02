using Terraria.Localization;
using Terraria.ModLoader;

namespace UnoClass.Utilities
{
	public static class LangHelper
	{
		public static string GetTextByMod(Mod mod, string key, params object[] args)
		{
			return Language.GetTextValue($"Mods.{mod.Name}.{key}", args);
		}
		public static LocalizedText GetLocalizationByMod(Mod mod, string key)
		{
			return Language.GetText($"Mods.{mod.Name}.{key}");
		}
		internal static string GetText(string key, params object[] args)
		{
			return GetTextByMod(UnoClass.mod, key, args);
		}
		internal static LocalizedText GetLocalization(string key)
		{
			return GetLocalizationByMod(UnoClass.mod, key);
		}
	}
}
