using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogMonitoring
{
	/// <summary>
	/// 設問１
	/// </summary>
	/// <remarks>
	/// 監視ログファイルを読み込み、故障状態のサーバアドレスとそのサーバの故障期間を出力するプログラムを作成せよ。
	/// 出力フォーマットは任意でよい。
	/// なお、pingがタイムアウトした場合を故障とみなし、最初にタイムアウトしたときから、次にpingの応答が返るまでを故障期間とする。
	/// </remarks>
	internal class Question1
	{
		/// <summary>
		/// 解答処理
		/// </summary>
		public static void Answer()
		{
			List<string[]> timeoutList = new List<string[]>();
			using (StreamReader sr = new StreamReader(Util.TargetLogFileFullPath))
			{
				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();
					string[] values = line.Split(',');

					//タイムアウトリスト内の一致するインデックスを取得
					int index = GetMatchIndexTimeoutList(timeoutList, values[Util.Cols.ServerAddress]);

					if (index < 0 && values[Util.Cols.ResponseResult].Equals("-"))
					{
						//タイムアウトリスト内に一致するサーバーアドレスが存在しない場合かつ、
						//読み込み中のログがタイムアウトの場合はリストに追加
						timeoutList.Add(values);
					}
					else if (index >= 0 && !values[Util.Cols.ResponseResult].Equals("-"))
					{
						//タイムアウトリスト内に一致するサーバーアドレスが存在する場合かつ、
						//読み込み中のログがタイムアウトでない場合は停止期間を出力
						Util.CalcDatetimeDiff dtInfo = new Util.CalcDatetimeDiff(timeoutList[index][Util.Cols.ChackDateTime], values[Util.Cols.ChackDateTime]);
						StringBuilder sb = new StringBuilder();
						sb.Append(values[Util.Cols.ServerAddress] + " ");
						sb.Append(dtInfo.From + "～" + dtInfo.To);
						sb.Append($"({dtInfo.Diff:%d}日{dtInfo.Diff:%h}時間{dtInfo.Diff:%m}分{dtInfo.Diff:%s}秒)");
						Console.WriteLine(sb.ToString());
						//タイムアウトリストから削除する
						timeoutList.RemoveAt(index);
					}
				}
				//応答がないままのサーバーは現在日時まで応答なしとみなす
				foreach (string[] values in timeoutList)
				{
					Util.CalcDatetimeDiff dtInfo = new Util.CalcDatetimeDiff(values[Util.Cols.ChackDateTime]);
					StringBuilder sb = new StringBuilder();
					sb.Append(values[Util.Cols.ServerAddress] + " ");
					sb.Append(dtInfo.From + "～" + dtInfo.To);
					sb.Append($"({dtInfo.Diff:%d}日{dtInfo.Diff:%h}時間{dtInfo.Diff:%m}分{dtInfo.Diff:%s}秒)");
					Console.WriteLine(sb.ToString());
				}
			}
		}

		/// <summary>
		/// タイムアウトリスト内からサーバーアドレスが一致するインデックスを取得する
		/// </summary>
		/// <param name="vTimeoutList">タイムアウトリスト</param>
		/// <param name="vTargetServerAddress">対象サーバーアドレス</param>
		/// <returns>一致するインデックス（存在しない場合は-1を返す）</returns>
		private static int GetMatchIndexTimeoutList(List<string[]> vTimeoutList, string vTargetServerAddress)
		{
			for (int i = 0; i < vTimeoutList.Count; i++)
			{
				if (vTimeoutList[i][Util.Cols.ServerAddress].Equals(vTargetServerAddress))
				{
					return i;
				}
			}
			return -1;
		}
	}
}
