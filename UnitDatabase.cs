﻿// <copyright file="UnitDatabase.cs" company="EnsageSharp">
//    Copyright (c) 2015 EnsageSharp.
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/
// </copyright>

namespace Ensage.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Ensage.Common.Properties;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    ///     The unit database.
    /// </summary>
    public static class UnitDatabase
    {
        #region Static Fields

        /// <summary>
        ///     The units.
        /// </summary>
        public static List<AttackAnimationData> Units;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="UnitDatabase" /> class.
        /// </summary>
        static UnitDatabase()
        {
            JToken @object;
            if (JObject.Parse(Encoding.Default.GetString(Resources.UnitDatabase)).TryGetValue("Units", out @object))
            {
                Units = JsonConvert.DeserializeObject<AttackAnimationData[]>(@object.ToString()).ToList();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the attack backswing.
        /// </summary>
        /// <param name="unit">
        ///     The unit.
        /// </param>
        /// <returns>
        ///     The <see cref="double" />.
        /// </returns>
        public static double GetAttackBackswing(Hero unit)
        {
            var attackRate = GetAttackRate(unit);
            var attackPoint = GetAttackPoint(unit);
            return attackRate - attackPoint;
        }

        /// <summary>
        ///     Gets the attack point.
        /// </summary>
        /// <param name="unit">
        ///     The unit.
        /// </param>
        /// <returns>
        ///     The <see cref="double" />.
        /// </returns>
        public static double GetAttackPoint(Hero unit)
        {
            if (unit == null)
            {
                return 0;
            }
            var name = unit.Name;
            var attackAnimationPoint =
                Game.FindKeyValues(name + "/AttackAnimationPoint", KeyValueSource.Hero).FloatValue;
            var attackSpeed = GetAttackSpeed(unit);
            return attackAnimationPoint / (1 + (attackSpeed - 100) / 100);
        }

        /// <summary>
        ///     Gets the attack rate.
        /// </summary>
        /// <param name="unit">
        ///     The unit.
        /// </param>
        /// <returns>
        ///     The <see cref="double" />.
        /// </returns>
        public static double GetAttackRate(Hero unit)
        {
            var attackSpeed = GetAttackSpeed(unit);
            var attackBaseTime = Game.FindKeyValues(unit.Name + "/AttackRate", KeyValueSource.Hero).FloatValue;
            Ability spell = null;
            if (
                !unit.Modifiers.Any(
                    x =>
                    (x.Name == "modifier_alchemist_chemical_rage" || x.Name == "modifier_terrorblade_metamorphosis"
                     || x.Name == "modifier_lone_druid_true_form" || x.Name == "modifier_troll_warlord_berserkers_rage")))
            {
                return attackBaseTime / (1 + (attackSpeed - 100) / 100);
            }

            switch (unit.ClassID)
            {
                case ClassID.CDOTA_Unit_Hero_Alchemist:
                    spell = unit.Spellbook.Spells.First(x => x.Name == "alchemist_chemical_rage");
                    break;
                case ClassID.CDOTA_Unit_Hero_Terrorblade:
                    spell = unit.Spellbook.Spells.First(x => x.Name == "terrorblade_metamorphosis");
                    break;
                case ClassID.CDOTA_Unit_Hero_LoneDruid:
                    spell = unit.Spellbook.Spells.First(x => x.Name == "lone_druid_true_form");
                    break;
                case ClassID.CDOTA_Unit_Hero_TrollWarlord:
                    spell = unit.Spellbook.Spells.First(x => x.Name == "troll_warlord_berserkers_rage");
                    break;
            }

            if (spell == null)
            {
                return attackBaseTime / (1 + (attackSpeed - 100) / 100);
            }
            var baseAttackTime = spell.AbilityData.FirstOrDefault(x => x.Name == "base_attack_time");
            if (baseAttackTime != null)
            {
                attackBaseTime = baseAttackTime.GetValue(spell.Level - 1);
            }

            return attackBaseTime / (1 + (attackSpeed - 100) / 100);
        }

        /// <summary>
        ///     Gets the attack speed.
        /// </summary>
        /// <param name="unit">
        ///     The unit.
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        public static float GetAttackSpeed(Hero unit)
        {
            //Console.WriteLine(unit.AttacksPerSecond * Game.FindKeyValues(unit.Name + "/AttackRate", KeyValueSource.Hero).FloatValue / 0.01);
            var attackSpeed =
                Math.Min(
                    unit.AttacksPerSecond
                    * Game.FindKeyValues(unit.Name + "/AttackRate", KeyValueSource.Hero).FloatValue / 0.01,
                    600);

            if (unit.Modifiers.Any(x => (x.Name == "modifier_ursa_overpower")))
            {
                attackSpeed = 600;
            }

            return (float)attackSpeed;
        }

        /// <summary>
        ///     Gets the attack animation data by class id.
        /// </summary>
        /// <param name="classId">
        ///     The class id.
        /// </param>
        /// <returns>
        ///     The <see cref="AttackAnimationData" />.
        /// </returns>
        public static AttackAnimationData GetByClassId(ClassID classId)
        {
            return Units.FirstOrDefault(unitData => unitData.UnitClassId.Equals(classId));
        }

        /// <summary>
        ///     Gets the attack animation data by name.
        /// </summary>
        /// <param name="unitName">
        ///     The unit name.
        /// </param>
        /// <returns>
        ///     The <see cref="AttackAnimationData" />.
        /// </returns>
        public static AttackAnimationData GetByName(string unitName)
        {
            return Units.FirstOrDefault(unitData => unitData.UnitName.ToLower() == unitName);
        }

        /// <summary>
        ///     Returns units projectile speed
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double GetProjectileSpeed(Hero unit)
        {
            if (unit == null || !unit.IsRanged)
            {
                return double.MaxValue;
            }
            var name = unit.Name;
            try
            {
                return Game.FindKeyValues(name + "/ProjectileSpeed", KeyValueSource.Hero).FloatValue;
            }
            catch (Exception)
            {
                return double.MaxValue;
            }
        }

        #endregion
    }
}