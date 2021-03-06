﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace py2km.api
{
	public class Cedict
	{
		// Make it outside, load once, to RAM
		static Dictionary<string, CedictData> CEDICT_TC = new System.Collections.Generic.Dictionary<string, CedictData>();
        static Dictionary<string, CedictData> CEDICT_SC = new System.Collections.Generic.Dictionary<string, CedictData>();

        // Total Listed CEDICT
        public static int COUNT_SC;
        public static int COUNT_TC;

		public static void Load()
		{
			foreach (var item in File.ReadAllLines("cedict_ts.u8"))
			{
				CedictData CedictContent = new CedictData()
				{
					Pinyin = null,
					English = null
				};

				if (item[0] == '#')
					continue;

				string tc = null;
				string sc = null;
				string py = null;
				string en = null;
				int i = 0;

				// Get Traditional Chinese
				while (item[i] != ' ')
				{
					tc += item[i++];
				}

				i++;

				// Get Simplified Chinese
				while (item[i] != ' ')
				{
					sc += item[i++];
				}

				i++;
				i++;

				// Get Pinyin (number)
				while (item[i] != ']')
				{
					py += item[i++];
				}

				i++;
				i++;
				i++;

				// Get English
				while (item.Length != i + 1)
				{
					en += item[i];
					i++;
				}

				// Drop non dict info, eg: surname
				if (en.ToLower().Contains("surname"))
					continue;

				// Add to dictionary
				CedictContent.Pinyin = py;
				CedictContent.English = en;

                // Traditional Chinese
                if (!CEDICT_TC.ContainsKey(tc))
                {
                    CedictContent.Chinese = tc;
                    CEDICT_TC.Add(tc, CedictContent);
                }

                // Simplified Chinese
                if (!CEDICT_SC.ContainsKey(sc))
                {
                    CedictContent.Chinese = sc;
                    CEDICT_SC.Add(sc, CedictContent);
                }
			}

            COUNT_TC = CEDICT_TC.Count;
            COUNT_SC = CEDICT_SC.Count;
		}

		// Below converting to Pinyin (number)
		public static string ToDict(string input)
		{
			string Tx = input;
			string Fi = null;

			int idx = 0; // Index
			int pos = Tx.Length; // Current position
			int len = Tx.Length; // Current length

			while (len != 0)
			{
				CedictData test;
				string temp = Tx.Substring(idx, len);
                if (CEDICT_TC.TryGetValue(temp, out test) || CEDICT_SC.TryGetValue(temp, out test))
				{
					string hanzi = test.Chinese;
					string pinyin = Converter.ToneToPinyin(test.Pinyin);
					string kwikman = Converter.PinyinToKwikMandarin(Converter.ToneToPinyin(test.Pinyin));
					string english = test.English;
					string uri = Uri.EscapeDataString(hanzi);

					// Make HTML based result
					string holder = Properties.Resources.HtmlContentDict;
					Fi += String.Format(holder, hanzi, pinyin, kwikman, english, uri);

					idx = pos; // Once found, move index to current position
					pos = Tx.Length; // then new position restart
					len = pos - idx; // then new length
				}
				else
				{
					len--; // Length of string
					pos--; // Go next character
					if (idx == pos)
					{
						idx++;
						pos = Tx.Length;
						len = pos - idx;
					}
				}
			}
			return Fi + Properties.Resources.HtmlContentJsAudio;
		}

		// Below converting to Pinyin (number)
		public static string ToPinyin(string input)
		{
			string Tx = input;
			string Fi = null;

			int idx = 0; // Index
			int pos = Tx.Length; // Current position
			int len = Tx.Length; // Current length

			while (len != 0)
			{
				CedictData test;
				string temp = Tx.Substring(idx, len);
                if (CEDICT_TC.TryGetValue(temp, out test) || CEDICT_SC.TryGetValue(temp, out test))
				{
					Fi += test.Pinyin + " ";

					idx = pos; // Once found, move index to current position
					pos = Tx.Length; // then new position restart
					len = pos - idx; // then new length
				}
				else
				{
					len--; // Length of string
					pos--; // Go next character
					if (idx == pos)
					{
						Fi += Tx.Substring(pos, 1); // Copy unknown character, such as period, comma, symbols
						idx++;
						pos = Tx.Length;
						len = pos - idx;
					}
				}
			}
			return Fi.ToLower();
		}

        public static string[] ToFlashCard(int pos)
        {
            string[] m = new string[3];
            int i = 0;
            foreach (var item in CEDICT_SC)
            {
                if (i++ == pos)
                {
                    m[0] = item.Value.Chinese;
                    m[1] = item.Value.Pinyin;
                    m[2] = item.Value.English;
                    break;
                }
            }
            return m;
        }
	}

	public class CedictData
	{
		public string Chinese { get; set; }
		public string Pinyin { get; set; }
		public string English { get; set; }
	}
}
