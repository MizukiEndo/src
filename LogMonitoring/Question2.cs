using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogMonitoring
{
	/// <summary>
	/// 設問２
	/// </summary>
	/// <remarks>
	/// ネットワークの状態によっては、一時的にpingがタイムアウトしても、一定期間するとpingの応答が復活することがあり、
	/// そのような場合はサーバの故障とみなさないようにしたい。
	/// N回以上連続してタイムアウトした場合にのみ故障とみなすように、設問1のプログラムを拡張せよ。
	/// Nはプログラムのパラメータとして与えられるようにすること。
	/// </remarks>
	internal class Question2
	{
		/// <summary>
		/// 解答処理
		/// </summary>
		/// <param name="vTimeoutCnt">故障とする連続タイムアウト回数</param>
		public static void Answer(int vTimeoutCnt)
		{
			const int ColsTimeoutCont = 3;
			bool isDataExists = false;
			List<List<string>> timeoutList = new List<List<string>>();
			using (StreamReader sr = new StreamReader(Util.TargetLogFileFullPath))
			{
				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();
					List<string> values = new List<string>(line.Split(','));

					//タイムアウトリスト内の一致するインデックスを取得
					int index = GetMatchIndexTimeoutList(timeoutList, values[Util.Cols.ServerAddress]);

					if (index < 0 && values[Util.Cols.ResponseResult].Equals("-"))
					{
						//タイムアウトリスト内に一致するサーバーアドレスが存在しない場合かつ、
						//読み込み中のログがタイムアウトの場合はリストに追加
						values.Add("1");
						timeoutList.Add(values);
					}
					else if (index >= 0 && values[Util.Cols.ResponseResult].Equals("-"))
					{
						//タイムアウトリスト内に一致するサーバーアドレスが存在する場合かつ、
						//読み込み中のログがタイムアウトの場合、タイムアウト数をカウントアップ
						int cnt = Convert.ToInt32(timeoutList[index][ColsTimeoutCont]) + 1;
						timeoutList[index][ColsTimeoutCont] = cnt.ToString();
					}
					else if (index >= 0 && !values[Util.Cols.ResponseResult].Equals("-"))
					{
						//タイムアウトリスト内に一致するサーバーアドレスが存在する場合かつ、
						//読み込み中のログがタイムアウトでない場合、引数回数以上停止していたら出力
						if (Convert.ToInt32(timeoutList[index][ColsTimeoutCont]) >= vTimeoutCnt)
						{
							Util.CalcDatetimeDiff dtInfo = new Util.CalcDatetimeDiff(timeoutList[index][Util.Cols.ChackDateTime], values[Util.Cols.ChackDateTime]);
							StringBuilder sb = new StringBuilder();
							sb.Append(values[Util.Cols.ServerAddress] + " ");
							sb.Append(dtInfo.From + "～" + dtInfo.To);
							sb.Append($"({dtInfo.Diff:%d}日{dtInfo.Diff:%h}時間{dtInfo.Diff:%m}分{dtInfo.Diff:%s}秒 {timeoutList[index][ColsTimeoutCont]}回)");
							Console.WriteLine(sb.ToString());
							isDataExists = true;
						}
						//タイムアウトリストから削除する
						timeoutList.RemoveAt(index);
					}
				}

				//応答がないままのサーバーは現在日時まで応答なしとみなす
				foreach (List<string> values in timeoutList)
				{
					if (Convert.ToInt32(values[ColsTimeoutCont]) >= vTimeoutCnt)
					{
						Util.CalcDatetimeDiff dtInfo = new Util.CalcDatetimeDiff(values[Util.Cols.ChackDateTime]);
						StringBuilder sb = new StringBuilder();
						sb.Append(values[Util.Cols.ServerAddress] + " ");
						sb.Append(dtInfo.From + "～" + dtInfo.To);
						sb.Append($"({dtInfo.Diff:%d}日{dtInfo.Diff:%h}時間{dtInfo.Diff:%m}分{dtInfo.Diff:%s}秒)");
						Console.WriteLine(sb.ToString());
						isDataExists = true;
					}
				}
			}
			if (!isDataExists)
			{
				Console.WriteLine("該当データはありませんでした。");
			}
		}

		/// <summary>
		/// タイムアウトリスト内からサーバーアドレスが一致するインデックスを取得する
		/// </summary>
		/// <param name="vTimeoutList">タイムアウトリスト</param>
		/// <param name="vTargetServerAddress">対象サーバーアドレス</param>
		/// <returns>一致するインデックス（存在しない場合は-1を返す）</returns>
		private static int GetMatchIndexTimeoutList(List<List<string>> vTimeoutList, string vTargetServerAddress)
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
