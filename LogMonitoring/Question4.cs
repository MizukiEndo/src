using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogMonitoring
{
	/// <summary>
	/// 設問４
	/// </summary>
	/// <remarks>
	/// ネットワーク経路にあるスイッチに障害が発生した場合、そのスイッチの配下にあるサーバの応答がすべてタイムアウトすると想定される。
	/// そこで、あるサブネット内のサーバが全て故障（ping応答がすべてN回以上連続でタイムアウト）している場合は、
	/// そのサブネット（のスイッチ）の故障とみなそう。
	/// 設問2または3のプログラムを拡張して、各サブネット毎にネットワークの故障期間を出力できるようにせよ
	/// </remarks>
	internal class Question4
	{

		/// <summary>
		/// チェック中データ保持クラス
		/// </summary>
		private class CheckData
		{
			public string ServerAddress = string.Empty;
			public List<string[]> LogList = new List<string[]>();

			/// <summary>
			/// サーバー毎の情報を保持
			/// </summary>
			/// <param name="values">ログ配列</param>
			public CheckData(string[] values)
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
			//チェック中データリスト
			List<CheckData> dataList = new List<CheckData>();
			using (StreamReader sr = new StreamReader(Util.TargetLogFileFullPath))
			{
				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();
					string[] values = line.Split(',');

					//チェック中データリスト内の一致するインデックスを取得
					int index = GetMatchIndexCheckDataList(dataList, values[Util.Cols.ServerAddress]);

					//応答なしのログは無視する
					if (values[Util.Cols.ResponseResult].Equals("-"))
					{
						continue;
					}

					if (index < 0)
					{
						//サーバーごとの初回処理
						CheckData data = new CheckData(values);
						dataList.Add(data);
						continue;
					}
					else
					{
						//サーバーごとのログを追加
						dataList[index].LogList.Add(values);
					}

					if (dataList[index].LogList.Count > vTargetCnt)
					{
						//該当サーバーの監視対象ログが引数指定値より多い場合、古いログを削除
						dataList[index].LogList.RemoveAt(0);
					}

					//監視対象ログが引数指定値より少ない場合は処理対象外
					if (dataList[index].LogList.Count < vTargetCnt)
					{
						continue;
					}

					//応答時間の平均値算出
					int res = 0;
					foreach (string[] log in dataList[index].LogList)
					{
						res += Convert.ToInt32(log[Util.Cols.ResponseResult]);
					}
					int ave = res / vTargetCnt;

					//応答時間平均値が引数指定値を超える場合、負荷状態として出力
					if (ave > vResponseBorder)
					{
						Util.CalcDatetimeDiff dtInfo = new Util.CalcDatetimeDiff(dataList[index].LogList[0][Util.Cols.ChackDateTime], values[Util.Cols.ChackDateTime]);
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
		/// チェック中データリスト内からサーバーアドレスが一致するインデックスを取得する
		/// </summary>
		/// <param name="vCheckDataList">チェック中データリスト</param>
		/// <param name="vTargetServerAddress">対象サーバーアドレス</param>
		/// <returns>一致するインデックス（存在しない場合は-1を返す）</returns>
		private static int GetMatchIndexCheckDataList(List<CheckData> vCheckDataList, string vTargetServerAddress)
		{
			for (int i = 0; i < vCheckDataList.Count; i++)
			{
				if (vCheckDataList[i].ServerAddress.Equals(vTargetServerAddress))
				{
					return i;
				}
			}
			return -1;
		}
	}
}
