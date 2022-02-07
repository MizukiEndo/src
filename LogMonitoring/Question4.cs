using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;

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
		/// サブネット毎のデータリスト
		/// </summary>
		private static List<SubnetData> SubnetList = new List<SubnetData>();

		#region クラス定義

		/// <summary>
		/// サブネット毎のデータを保持するクラス
		/// </summary>
		private class SubnetData
		{
			public string Subnet = string.Empty;
			public List<ServerData> ServerList = new List<ServerData>();

			/// <summary>
			/// 初回サブネット毎の情報を保持
			/// </summary>
			/// <param name="subnet">サブネット</param>
			/// <param name="values">ログ配列</param>
			public SubnetData(string subnet, string[] values)
			{
				this.Subnet = subnet;
				ServerData data = new ServerData(values);
				this.ServerList.Add(data);
			}
		}

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

		#endregion

		/// <summary>
		/// 解答処理
		/// </summary>
		/// <param name="vTargetCnt">対象とする監視回数</param>
		/// <param name="vResponseBorder">平均応答時間の許容下限値</param>
		public static void Answer(int vTargetCnt)
		{
			SubnetList = new List<SubnetData>();

			//サブネット毎のデータリストを作成
			MakeSubnetList();
			//サブネット毎に処理
			foreach (SubnetData subnetData in SubnetList)
			{
				bool isTimeout = false;
				int timeoutCnt = 0;
				string timeoutStartDt = string.Empty;

				//最初のサーバーデータを基準にログのタイムアウトを確認
				for (int i = 0; i < subnetData.ServerList[0].LogList.Count; i++)
				{
					string[] log = subnetData.ServerList[0].LogList[i];
					if (!isTimeout && !log[Util.Cols.ResponseResult].Equals("-"))
					{
						//タイムアウト中ではない場合はスキップ
						continue;
					}

					//同一サブネットの全サーバーのタイムアウト確認
					bool isSkip = false;
					foreach (ServerData serverData in subnetData.ServerList)
					{
						if (serverData.LogList.Count < i)
						{
							//インデックスエラー回避
							continue;
						}
						if (!serverData.LogList[i][Util.Cols.ResponseResult].Equals("-"))
						{
							//タイムアウトではないデータが存在した場合
							if (!isTimeout)
							{
								//タイムアウト中でないならスキップ
								isSkip = true;
							}
							else
							{
								//タイムアウト中の場合、タイムアウト終了処理
								if (timeoutCnt >= vTargetCnt)
								{
									//ネットワーク故障として出力
									Util.CalcDatetimeDiff dtInfo = new Util.CalcDatetimeDiff(timeoutStartDt, serverData.LogList[i][Util.Cols.ChackDateTime]);
									StringBuilder sb = new StringBuilder();
									sb.Append(subnetData.Subnet + " ");
									sb.Append(dtInfo.From + "～" + dtInfo.To);
									sb.Append($"({dtInfo.Diff:%d}日{dtInfo.Diff:%h}時間{dtInfo.Diff:%m}分{dtInfo.Diff:%s}秒 {timeoutCnt.ToString()}回)");
									Console.WriteLine(sb.ToString());
								}
								isTimeout = false;
								timeoutCnt = 0;
								isSkip = true;
							}
						}
					}

					if (isSkip)
					{
						continue;
					}
					else
					{
						//タイムアウト回数カウントアップ
						timeoutCnt += 1;
					}

					if (!isTimeout && log[Util.Cols.ResponseResult].Equals("-"))
					{
						//タイムアウト中ではなかったが、タイムアウトが発生した場合
						isTimeout = true;
						timeoutCnt = 1;
						timeoutStartDt = log[Util.Cols.ChackDateTime];
					}
				}

				//応答がないままのネットワークは現在日時まで応答なしとみなす
				if (isTimeout)
				{
					Util.CalcDatetimeDiff dtInfo = new Util.CalcDatetimeDiff(timeoutStartDt);
					StringBuilder sb = new StringBuilder();
					sb.Append(subnetData.Subnet + " ");
					sb.Append(dtInfo.From + "～" + dtInfo.To);
					sb.Append($"({dtInfo.Diff:%d}日{dtInfo.Diff:%h}時間{dtInfo.Diff:%m}分{dtInfo.Diff:%s}秒 {timeoutCnt.ToString()}回)");
					Console.WriteLine(sb.ToString());
				}
			}
		}

		/// <summary>
		/// サブネット毎のデータリスト作成
		/// </summary>
		private static void MakeSubnetList()
		{
			using (StreamReader sr = new StreamReader(Util.TargetLogFileFullPath))
			{
				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();
					string[] values = line.Split(',');

					//IPアドレスを文字列⇒IPNetwork(NuGetパッケージからdll追加)変換
					IPNetwork ip = IPNetwork.Parse(values[Util.Cols.ServerAddress]);

					//サブネット毎のデータリスト作成
					int subnetIndex = GetMatchIndexSubnetDataList(ip.Netmask.ToString());
					if (subnetIndex < 0)
					{
						//サブネットデータ新規追加
						SubnetData subnetData = new SubnetData(ip.Netmask.ToString(), values);
						SubnetList.Add(subnetData);
					}
					else
					{
						int serverIndex = GetMatchIndexServerDataList(SubnetList[subnetIndex].ServerList, values[Util.Cols.ServerAddress]);
						if (serverIndex < 0)
						{
							//既存のサブネットデータのサーバーリストに新しいサーバーデータを追加
							ServerData serverData = new ServerData(values);
							SubnetList[subnetIndex].ServerList.Add(serverData);
						}
						else
						{
							//既存のサーバーデータにログを追加
							SubnetList[subnetIndex].ServerList[serverIndex].LogList.Add(values);
						}
					}
				}
			}
		}

		#region リスト内の一致するインデックスを取得する

		/// <summary>
		/// サブネット毎のデータリスト内からサブネットが一致するインデックスを取得する
		/// </summary>
		/// <param name="vSubnetList">サブネット毎のデータリスト</param>
		/// <param name="vTargetSubnet">対象サブネット</param>
		/// <returns>一致するインデックス（存在しない場合は-1を返す）</returns>
		private static int GetMatchIndexSubnetDataList(string vTargetSubnet)
		{
			for (int i = 0; i < SubnetList.Count; i++)
			{
				if (SubnetList[i].Subnet.Equals(vTargetSubnet))
				{
					return i;
				}
			}
			return -1;
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

		#endregion
	}
}
