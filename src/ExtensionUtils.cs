
namespace com.convalise.Lib
{

public static class ExtensionUtils
{

	#region UTF8_STRING

	/// <summary>
	/// Converts a byte array to its hex string counterpart.
	/// </summary>
	public static string ToHexString(this byte[] byteArray)
	{
		return System.BitConverter.ToString(byteArray).Replace("-", string.Empty);
	}

	/// <summary>
	/// Converts a byte array to its hex string counterpart.
	/// </summary>
	public static string ToHexString(this byte[] byteArray, int startIndex, int length)
	{
		return System.BitConverter.ToString(byteArray, startIndex, length).Replace("-", string.Empty);
	}

	/// <summary>
	/// Reads a UTF8 formatted string from a byte array.
	/// </summary>
	public static string ToUTF8String(this byte[] byteArray)
	{
		return System.Text.Encoding.UTF8.GetString(byteArray).Trim('\0');
	}
	
	/// <summary>
	/// Reads a UTF8 formatted string from a byte array.
	/// </summary>
	public static string ToUTF8String(this byte[] byteArray, int index, int count)
	{
		return System.Text.Encoding.UTF8.GetString(byteArray, index, count).Trim('\0');
	}

	#endregion

	#region DEC_TO_HEX

	/// <summary>
	/// Converts the decimal number to a hex string.
	/// </summary>
	public static string ToHexString(this byte @decimal, bool pad = true)
	{
		string hexString = @decimal.ToString("X");
		return pad ? ZeroPad(hexString) : hexString;
	}
	
	/// <summary>
	/// Converts the decimal number to a hex string.
	/// </summary>
	public static string ToHexString(this short @decimal, bool pad = true)
	{
		string hexString = @decimal.ToString("X");
		return pad ? ZeroPad(hexString) : hexString;
	}
	
	/// <summary>
	/// Converts the decimal number to a hex string.
	/// </summary>
	public static string ToHexString(this int @decimal, bool pad = true)
	{
		string hexString = @decimal.ToString("X");
		return pad ? ZeroPad(hexString) : hexString;
	}
	
	/// <summary>
	/// Converts the decimal number to a hex string.
	/// </summary>
	public static string ToHexString(this long @decimal, bool pad = true)
	{
		string hexString = @decimal.ToString("X");
		return pad ? ZeroPad(hexString) : hexString;
	}

	/// <summary>
	/// Zero-pad the hex string if it has an odd number of chars.
	/// </summary>
	private static string ZeroPad(string hexString)
	{
		return ((hexString.Length % 2) != 0) ? ("0" + hexString) : hexString;
	}

	#endregion

	#region HEX_TO_DEC
	
	/// <summary>
	/// Converts a hex string to a base 8 decimal.
	/// </summary>
	public static byte ToDecimal8(this string hexString)
	{
		return System.Convert.ToByte(hexString, 16);
	}
	
	/// <summary>
	/// Converts a hex string to a base 16 decimal.
	/// </summary>
	public static short ToDecimal16(this string hexString)
	{
		return System.Convert.ToInt16(hexString, 16);
	}
	
	/// <summary>
	/// Converts a hex string to a base 32 decimal.
	/// </summary>
	public static int ToDecimal32(this string hexString)
	{
		return System.Convert.ToInt32(hexString, 16);
	}
	
	/// <summary>
	/// Converts a hex string to a base 64 decimal.
	/// </summary>
	public static long ToDecimal64(this string hexString)
	{
		return System.Convert.ToInt64(hexString, 16);
	}

	#endregion

}

} /// End of namespace.
