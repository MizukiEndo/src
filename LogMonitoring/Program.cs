using System;

namespace LogMonitoring
{
	/// <summary>
	/// ログ監視
	/// </summary>
	/// <remarks>
	/// ◆◇◆フィックスポイント社様課題◆◇◆
	/// A社の監視システムでは、監視対象となる複数台のサーバに対して一定間隔でping応答確認を行っており、
	/// 確認結果は以下に示すカンマ区切りの形式で1行ずつ監視ログファイルに追記される。
	/// -------------------------------------------------
	/// ＜確認日時＞,＜サーバアドレス＞,＜応答結果＞
	/// -------------------------------------------------
	/// 確認日時は、YYYYMMDDhhmmssの形式。ただし、年＝YYYY（4桁の数字）、月＝MM（2桁の数字。以下同様）、日＝DD、時＝hh、分＝mm、秒＝ssである。
	/// サーバアドレスは、ネットワークプレフィックス長付きのIPv4アドレスである。
	/// 応答結果には、pingの応答時間がミリ秒単位で記載される。ただし、タイムアウトした場合は"-"(ハイフン記号) となる。
	/// </remarks>
	internal class Program
	{
		/// <summary>
		/// メイン処理
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			try
			{
				ShowMenu();
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
				Console.ReadKey();
			}
		}

		/// <summary>
		/// メニュー表示
		/// </summary>
		private static void ShowMenu()
		{
			Console.WriteLine();
			Console.WriteLine("サーバー監視ログを参照します。処理を選択してください。");
			Console.WriteLine("1.すべての故障情報を見る（設問１）");
			Console.WriteLine("2.自動復活を除く故障情報を見る（設問２）");
			Console.WriteLine("3.負荷情報を見る（設問３）");
			Console.WriteLine("4.サブネット毎の故障情報を見る（設問４）");
			Console.WriteLine("0.終了する");
			string key = Console.ReadLine();
			switch (key)
			{
				case "1":
					Question1.Answer();
					ShowMenu();
					break;
				case "2":
					Console.WriteLine("故障とみなすタイムアウト回数を指定してください。");
					string val = Console.ReadLine();
					if (int.TryParse(val, out int timeoutCnt))
					{
						Question2.Answer(timeoutCnt);
					}
					else
					{
						Console.WriteLine("入力値が無効です。");
					}
					ShowMenu();
					break;
				case "3":
					Console.WriteLine("応答時間の平均値を算出するログ数をしてしてください。");
					string val1 = Console.ReadLine();
					Console.WriteLine("負荷とみなさない応答時間の上限値を指定してください。");
					string val2 = Console.ReadLine();
					if (int.TryParse(val1, out int targetCnt) && int.TryParse(val2, out int responseBorder) && targetCnt > 0)
					{
						Question3.Answer(targetCnt, responseBorder);
					}
					else
					{
						Console.WriteLine("入力値が無効です。");
					}
					ShowMenu();
					break;
				case "4":
					ShowMenu();
					break;
				case "0":
					break;
				default:
					ShowMenu();
					break;
			}
		}
	}
}