using System;
using System.Configuration;
using System.Globalization;

namespace LogMonitoring
{
	/// <summary>
	/// 共通定義
	/// </summary>
	internal class Util
	{
		/// <summary>
		/// 監視対象ログファイルフルパス（設定ファイルより）
		/// </summary>
		public static string TargetLogFileFullPath = ConfigurationManager.AppSettings["TargetLogFileFullPath"];

		/// <summary>
		/// ログファイル列定義
		/// </summary>
		public static class Cols
		{
			public const int ChackDateTime = 0;
			public const int ServerAddress = 1;
			public const int ResponseResult = 2;
		}

		/// <summary>
		/// 日時の差を計算するクラス
		/// </summary>
		public class CalcDatetimeDiff
		{
			public string From;
			public string To;
			public TimeSpan Diff;

			/// <summary>
			/// 日時のTo-Fromを算出
			/// </summary>
			/// <param name="dtFrom">日時From</param>
			/// <param name="dtTo">日時To</param>
			public CalcDatetimeDiff(string vFrom, string vTo = "")
			{
				DateTime dtFrom = DateTime.ParseExact(vFrom, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
				DateTime dtTo;
				if (string.IsNullOrEmpty(vTo))
				{
					dtTo = DateTime.Now;
				}
				else
				{
					dtTo = DateTime.ParseExact(vTo, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
				}
				this.From = dtFrom.ToString("yyyy/MM/dd HH:mm:ss");
				this.To = dtTo.ToString("yyyy/MM/dd HH:mm:ss");
				this.Diff = dtTo - dtFrom;
			}
		}
	}
}
