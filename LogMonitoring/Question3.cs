using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogMonitoring
{
	/// <summary>
	/// 設問３
	/// </summary>
	/// <remarks>
	/// サーバが返すpingの応答時間が長くなる場合、サーバが過負荷状態になっていると考えられる。
	/// そこで、直近m回の平均応答時間がtミリ秒を超えた場合は、サーバが過負荷状態になっているとみなそう。
	/// 設問2のプログラムを拡張して、各サーバの過負荷状態となっている期間を出力できるようにせよ。
	/// mとtはプログラムのパラメータとして与えられるようにすること。
	/// </remarks>
	internal class Question3
	{
		/// <summary>
		/// サーバー毎のデータ保持するクラス
		/// </summary>
		private class ServerData
		{
			public string ServerAddress = string.Empty;
			public List<string[]> LogList = new List<string[]>();

			/// <summary>
			/// 初回サーバー情報を保持
			/// </summary>
			/// <param name="values">ログ配列</param>
			public ServerData(string[] values)
			{
				this.ServerAddress = values[Util.Cols.ServerAddress];
				this.LogList.Add(values);
			}
		}

		/// <summary>
		/// 解答処理
		/// </summary>
		/// <param name="vTargetCnt">対象とする監視回数</param>
		/// <param name="vResponseBorder">平均応答時間の許容下限値</param>
		public static void Answer(int vTargetCnt, int vResponseBorder)
		{
			//サーバー毎のデータリスト
			List<ServerData> serverList = new List<ServerData>();
			using (StreamReader sr = new StreamReader(Util.TargetLogFileFullPath))
			{
				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();
					string[] values = line.Split(',');

					//サーバー毎のデータリスト内の一致するインデックスを取得
					int index = GetMatchIndexServerDataList(serverList, values[Util.Cols.ServerAddress]);

					//応答なしのログは無視する
					if (values[Util.Cols.ResponseResult].Equals("-"))
					{
						continue;
					}

					if (index < 0)
					{
						//サーバーごとの初回処理
						ServerData data = new ServerData(values);
						serverList.Add(data);
						continue;
					}
					else
					{
						//サーバーごとのログを追加
						serverList[index].LogList.Add(values);
					}

					if (serverList[index].LogList.Count > vTargetCnt)
					{
						//該当サーバーの監視対象ログが引数指定値より多い場合、古いログを削除
						serverList[index].LogList.RemoveAt(0);
					}

					//監視対象ログが引数指定値より少ない場合は処理対象外
					if (serverList[index].LogList.Count < vTargetCnt)
					{
						continue;
					}

					//応答時間の平均値算出
					int res = 0;
					foreach (string[] log in serverList[index].LogList)
					{
						res += Convert.ToInt32(log[Util.Cols.ResponseResult]);
					}
					int ave = res / vTargetCnt;

					//応答時間平均値が引数指定値を超える場合、負荷状態として出力
					if (ave > vResponseBorder)
					{
						Util.CalcDatetimeDiff dtInfo = new Util.CalcDatetimeDiff(serverList[index].LogList[0][Util.Cols.ChackDateTime], values[Util.Cols.ChackDateTime]);
						StringBuilder sb = new StringBuilder();
						sb.Append(values[Util.Cols.ServerAddress] + " ");
						sb.Append(dtInfo.From + "～" + dtInfo.To);
						sb.Append($"({dtInfo.Diff:%d}日{dtInfo.Diff:%h}時間{dtInfo.Diff:%m}分{dtInfo.Diff:%s}秒 平均応答時間:{ave.ToString()}秒)");
						Console.WriteLine(sb.ToString());
					}
				}
			}
		}

		/// <summary>
		/// サーバー毎のデータリスト内からサーバーアドレスが一致するインデックスを取得する
		/// </summary>
		/// <param name="vServerDataList">サーバー毎のデータリスト</param>
		/// <param name="vTargetServerAddress">対象サーバーアドレス</param>
		/// <returns>一致するインデックス（存在しない場合は-1を返す）</returns>
		private static int GetMatchIndexServerDataList(List<ServerData> vServerList, string vTargetServerAddress)
		{
			for (int i = 0; i < vServerList.Count; i++)
			{
				if (vServerList[i].ServerAddress.Equals(vTargetServerAddress))
				{
					return i;
				}
			}
			return -1;
		}
	}
}
