using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WeaponPaints
{
	internal static class Utility
	{
		internal static WeaponPaintsConfig? Config { get; set; }

		internal static async Task CheckDatabaseTables()
		{
			if (WeaponPaints._database is null) return;

			try
			{
				await using var connection = await WeaponPaints._database.GetConnectionAsync();

				await using var transaction = await connection.BeginTransactionAsync();

				try
				{
					string[] createTableQueries =
					[
						"""
						CREATE TABLE IF NOT EXISTS `wp_player_skins` (
						                        `steamid` varchar(18) NOT NULL,
						                        `weapon_defindex` int(6) NOT NULL,
						                        `weapon_paint_id` int(6) NOT NULL,
						                        `weapon_wear` float NOT NULL DEFAULT 0.000001,
						                        `weapon_seed` int(16) NOT NULL DEFAULT 0
						                    ) ENGINE=InnoDB
						""",
						@"CREATE TABLE IF NOT EXISTS `wp_player_knife` (
                        `steamid` varchar(18) NOT NULL,
                        `knife` varchar(64) NOT NULL,
                        UNIQUE (`steamid`)
                    ) ENGINE = InnoDB",
						"""
						CREATE TABLE IF NOT EXISTS `wp_player_gloves` (
											 `steamid` varchar(18) NOT NULL,
											 `weapon_defindex` int(11) NOT NULL,
						                      UNIQUE (`steamid`)
											) ENGINE=InnoDB
						""",
						"""
						CREATE TABLE IF NOT EXISTS `wp_player_agents` (
											 `steamid` varchar(18) NOT NULL,
											 `agent_ct` varchar(64) DEFAULT NULL,
											 `agent_t` varchar(64) DEFAULT NULL,
											 UNIQUE (`steamid`)
											) ENGINE=InnoDB
						""",
						"""
						CREATE TABLE IF NOT EXISTS `wp_player_music` (
											 `steamid` varchar(64) NOT NULL,
											 `music_id` int(11) NOT NULL,
											 UNIQUE (`steamid`)
											) ENGINE=InnoDB
						""",
					];

					foreach (var query in createTableQueries)
					{
						await connection.ExecuteAsync(query, transaction: transaction);
					}

					await transaction.CommitAsync();
				}
				catch (Exception)
				{
					await transaction.RollbackAsync();
					throw new Exception("[WeaponPaints] Unable to create tables!");
				}
			}
			catch (Exception ex)
			{
				throw new Exception("[WeaponPaints] Unknown MySQL exception! " + ex.Message);
			}
		}

		internal static bool IsPlayerValid(CCSPlayerController? player)
		{
			if (player is null || WeaponPaints.weaponSync is null) return false;

			return player is { IsValid: true, IsBot: false, IsHLTV: false, UserId: not null };
		}

		internal static void LoadSkinsFromFile(string filePath, ILogger logger)
		{
			var json = File.ReadAllText(filePath);
			try
			{
				var deserializedSkins = JsonConvert.DeserializeObject<List<JObject>>(json);
				WeaponPaints.skinsList = deserializedSkins ?? [];
			}
			catch (FileNotFoundException)
			{
				logger?.LogError("Not found \"skins.json\" file");
			}
		}

		internal static void LoadGlovesFromFile(string filePath, ILogger logger)
		{
			try
			{
				var json = File.ReadAllText(filePath);
				var deserializedSkins = JsonConvert.DeserializeObject<List<JObject>>(json);
				WeaponPaints.glovesList = deserializedSkins ?? [];
			}
			catch (FileNotFoundException)
			{
				logger?.LogError("Not found \"gloves.json\" file");
			}
		}

		internal static void LoadAgentsFromFile(string filePath, ILogger logger)
		{
			try
			{
				var json = File.ReadAllText(filePath);
				var deserializedSkins = JsonConvert.DeserializeObject<List<JObject>>(json);
				WeaponPaints.agentsList = deserializedSkins ?? [];
			}
			catch (FileNotFoundException)
			{
				logger?.LogError("Not found \"agents.json\" file");
			}
		}

		internal static void LoadMusicFromFile(string filePath, ILogger logger)
		{
			try
			{
				var json = File.ReadAllText(filePath);
				var deserializedSkins = JsonConvert.DeserializeObject<List<JObject>>(json);
				WeaponPaints.musicList = deserializedSkins ?? [];
			}
			catch (FileNotFoundException)
			{
				logger?.LogError("Not found \"music.json\" file");
			}
		}

		internal static void Log(string message)
		{
			Console.BackgroundColor = ConsoleColor.DarkGray;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("[WeaponPaints] " + message);
			Console.ResetColor();
		}

		internal static string ReplaceTags(string message)
		{
			return message.ReplaceColorTags();
		}
	}
}