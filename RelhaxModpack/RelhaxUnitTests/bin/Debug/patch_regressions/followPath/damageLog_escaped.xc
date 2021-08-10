/**
 * Log of the received damage.
 * For additional settings see battleLabelsTemplates.xc
 * Лог полученного урона.
 * Дополнительные настройки см. в battleLabelsTemplates.xc
 * 
 * https://kr.cm/f/t/35169/

  Macros used in damageLog:
  Макросы используемые в damageLog:

    {{number}}         - line number / номер строки.
    {{dmg}}            - received damage / полученный урон.
    {{dmg-kind}}       - kind of the received damage (attack, fire, ramming, ...) / тип полученного урона (атака, пожар, таран, ...).
    {{c:dmg-kind}}     - color by damage kind / цвет по типу урона.
    {{hit-effects}}    - hit kind (with damage, ricochet, not penetrated, no damage) / тип попадания (с уроном, рикошет, не пробито, без урона).
    {{c:hit-effects}}  - color by hit kind / цвет по типу попадания.
    {{type-shell}}     - shell kind / тип снаряда.
    {{c:type-shell}}   - color by shell kind / цвет по типу снаряда.
    {{vtype}}          - vehicle type / тип техники.
    {{c:vtype}}        - color by vehicle type / цвет по типу техники.
    {{team-dmg}}       - team attachment of the attacker (ally , enemy, self damage) / командная принадлежность атакующего (союзник, противник, урон по себе).
    {{c:team-dmg}}     - color by team attachment of the attacker (ally , enemy, self damage) / цвет по командной принадлежности атакующего (союзник, противник, урон по себе).
    {{costShell}}      - shell currency (gold, credits) / валюта снаряда (золото, кредиты).
    {{c:costShell}}    - color by shell currency / цвет по валюте снаряда.
    {{vehicle}}        - attacker vehicle name / название техники атакующего.
    {{name}}           - attacker nickname / никнейм атакующего.
    {{critical-hit}}   - critical hit / критическое попадание.
    {{comp-name}}      - vehicle part that was hit (turret, hull, chassis, gun) / часть техники, в которую было попадание (башня, корпус, ходовая, орудие).
    {{clan}}           - clan name with brackets (empty if no clan) / название клана в скобках (пусто, если игрок не в клане).
    {{level}}          - vehicle level / уровень техники.
    {{clannb}}         - clan name without brackets / название клана без скобок.
    {{clanicon}}       - macro with clan emblem image path value / макрос со значением пути эмблемы клана.
    {{squad-num}}      - number of squad (1 ,2, ...), empty if not in squad / номер взвода (1, 2, ...), пусто - если игрок не во взводе.
    {{dmg-ratio}}      - received damage percent / полученный урон в процентах.
    {{splash-hit}}     - text for damage with shell splinters (HE/HESH) / текст при уроне осколками снаряда (ОФ/ХФ).
    {{my-alive}}       - value 'al' for alive own vehicle, '' for dead one / возвращает 'al', для живой собственной техники, '' для мертвой.
    {{reloadGun}}      - gun reloading time / время перезарядки орудия.
    {{gun-caliber}}    - gun caliber / калибр орудия.
    {{wn8}}, {{xwn8}}, {{wtr}}, {{xwtr}}, {{eff}}, {{xeff}}, {{wgr}}, {{xwgr}}, {{xte}}, {{r}}, {{xr}} - statistics macros (see macros.txt) / макросы статистики (смотрите macros_ru.txt).
    {{c:wn8}}, {{c:xwn8}}, {{c:wtr}}, {{c:xwtr}}, {{c:eff}}, {{c:xeff}}, {{c:wgr}}, {{c:xwgr}}, {{c:xte}}, {{c:r}}, {{c:xr}} - color according to the corresponding statistics macro (see macros.txt) / цвет по соответствующему макросу статистики (смотрите macros_ru.txt).
    {{fire-duration}}  - duration of fire ("groupDamagesFromFire" must be enabled to work) / продолжительность пожара (работает только при включенной опции "groupDamagesFromFire").
    {{diff-masses}}    - vehicles weights difference during collision / разность масс техники при столкновении.
    {{nation}}         - vehicle nation / нация техники.
    {{my-blownup}}     - value 'blownup' if own vehicle's ammunition have been blown up, '' otherwise / возвращает 'blownup', если взорван боекомплект собственной техники, иначе ''.
    {{stun-duration}}  - stun duration / продолжительность оглушения.
    {{crit-device}}    - damaged module or shell-shocked crew member / поврежденный модуль или контуженный член экипажа.
    {{type-shell-key}} - shell kind table key value / название ключа таблицы типа снаряда.
    {{hitTime}}        - time of the received (blocked) damage in "mm:ss" format / время полученного (заблокированного) урона в формате "мм:сс".
    {{vehiclename}}    - vehicle system name (usa-A34_M24_Chaffee) / название техники в системе (usa-A34_M24_Chaffee).
*/

{
  "damageLog": {
    // false - disable.
    // false - отключить.
    "enabled": true,
    // true - disable standard detailed damage.
    // true - отключить стандартный детальный урон.
    "disabledDetailStats": true,
    // true - disable standard summarized damage.
    // true - отключить стандартный суммарный урон.
    "disabledSummaryStats": true,
    // Log of the received damage.
    // Лог полученного урона.
    "log": {
      // true - allow to move log in battle and disallow macros for "x" and "y" settings.
      // false - disallow to move log in battle and allow macros for "x" and "y" settings.
      // true - разрешить перемещение лога в бою и запретить макросы в настройках "x" и "y".
      // false - запретить перемещение лога в бою и разрешить макросы в настройках "x" и "y".
      "moveInBattle": false,
      "x": 240,
      "y": -23,
      // true - show hits without damage.
      // true - отображать попадания без урона.
      "showHitNoDamage": true,
      // true - summarize damages from fire.
      // true - суммировать повреждения от пожара.
      "groupDamagesFromFire": true,
      // true - summarize damages from ramming, crash, falling (if more than one damage per second).
      // true - суммировать повреждения от тарана, столкновения, падения (если больше одного повреждения в секунду).
      "groupDamagesFromRamming_WorldCollision": true,
      // true - summarize damages from artillery strike and airstrike (if more than one damage per second).
      // true - суммировать повреждения от артудара и авионалета (если больше одного повреждения в секунду).
      "groupDamageFromArtAndAirstrike": true,
      // Kind of the received damage (macro {{dmg-kind}}).
      // Тип полученного урона (макрос {{dmg-kind}}).
      "dmg-kind": {
        "shot": "{{hit-effects}}{{critical-hit}}{{splash-hit}}<tab>{{type-shell}}",                        // shot / попадание.
        "fire": "{{hit-effects}}{{critical-hit}}<tab><font face='xvm'>&#x51;</font>",                      // fire / пожар.
        "ramming": "{{hit-effects}}{{critical-hit}}<tab><font face='xvm'>&#x52;</font>",                   // ramming / таран.
        "world_collision": "{{hit-effects}}{{critical-hit}}<tab><font face='xvm'>&#x53;</font>",           // world collision / столкновение с объектами, падение.
        "drowning": "{{l10n:drowning}}<tab><font face='xvm'>&#x119;</font>",                               // drowning / утопление.
        "overturn": "{{hit-effects}}<tab><font face='xvm'>&#x112;</font>",                                 // overturn / опрокидывание.
        "death_zone": "DZ",                                                                                // death zone / смертельная зона.
        "gas_attack": "GA",                                                                                // gas attack / газовая атака.
        "art_attack": "{{hit-effects}}{{critical-hit}}{{splash-hit}}<tab><font face='xvm'>&#x110;</font>", // art attack / артиллерийская поддержка.
        "air_strike": "{{hit-effects}}{{critical-hit}}{{splash-hit}}<tab><font face='xvm'>&#x111;</font>"  // air strike / поддержка авиации.
      },
      // Color by kind of the received damage (macro {{c:dmg-kind}}).
      // Цвет по типу полученного урона (макрос {{c:dmg-kind}}).
      "c:dmg-kind": {
        "shot": "{{c:hit-effects}}",       // shot / попадание.
        "fire": "#FF6655",                 // fire / пожар.
        "ramming": "#998855",              // ramming / таран.
        "world_collision": "#228855",      // world collision / столкновение с объектами, падение.
        "drowning": "#CCCCCC",             // drowning / утопление.
        "overturn": "#CCCCCC",             // overturn / опрокидывание.
        "death_zone": "#CCCCCC",           // death zone / смертельная зона.
        "gas_attack": "#CCCCCC",           // gas attack / газовая атака.
        "art_attack": "{{c:hit-effects}}", // art attack / артиллерийская поддержка.
        "air_strike": "{{c:hit-effects}}"  // air strike / поддержка авиации.
      },
      // Damage with shell splinters (HE/HESH). (macro {{splash-hit}}).
      // Урон осколками снаряда (ОФ/ХФ). (макрос {{splash-hit}}).
      "splash-hit": {
        "splash": "<font face='xvm'>&#x2C;</font>", // splash damage / попадание осколков.
        "no-splash": ""                             // no splash damage / нет попадания осколков.
      },
      // Shell kind (macro {{type-shell}}).
      // Тип снаряда (макрос {{type-shell}}).
      "type-shell": {
        "armor_piercing": "<font color='{{c:costShell}}'>{{l10n:armor_piercing}}</font>",       // armor piercing / бронебойный.
        "high_explosive": "<font color='{{c:costShell}}'>{{l10n:high_explosive}}</font>",       // high explosive / осколочно-фугасный.
        "armor_piercing_cr": "<font color='{{c:costShell}}'>{{l10n:armor_piercing_cr}}</font>", // armor piercing composite rigid / бронебойный подкалиберный.
        "armor_piercing_he": "<font color='{{c:costShell}}'>{{l10n:armor_piercing_he}}</font>", // armor piercing high explosive / бронебойно-фугасный.
        "hollow_charge": "<font color='{{c:costShell}}'>{{l10n:hollow_charge}}</font>",         // high explosive anti-tank / кумулятивный.
        "not_shell": ""                                                                         // another source of damage / другой источник урона.
      },
      // Color by shell kind (macro {{c:type-shell}}).
      // Цвет по типу снаряда (макрос {{c:type-shell}}).
      "c:type-shell": {
        "armor_piercing": "#CCCCCC",    // armor piercing / бронебойный.
        "high_explosive": "#CCCCCC",    // high explosive / осколочно-фугасный.
        "armor_piercing_cr": "#CCCCCC", // armor piercing composite rigid / бронебойный подкалиберный.
        "armor_piercing_he": "#CCCCCC", // armor piercing high explosive / бронебойно-фугасный.
        "hollow_charge": "#CCCCCC",     // high explosive anti-tank / кумулятивный.
        "not_shell": "#CCCCCC"          // another source of damage / другой источник урона.
      },
      // Vehicle type (macro {{vtype}}).
      // Тип техники (макрос {{vtype}}).
      "vtype": {
        "HT": "<font face='xvm'>&#x3F;</font>",  // heavy tank / тяжёлый танк.
        "MT": "<font face='xvm'>&#x3B;</font>",  // medium tank / средний танк.
        "LT": "<font face='xvm'>&#x3A;</font>",  // light tank / лёгкий танк.
        "TD": "<font face='xvm'>&#x2E;</font>",  // tank destroyer / ПТ-САУ.
        "SPG": "<font face='xvm'>&#x2D;</font>", // SPG / САУ.
        "not_vehicle": ""                        // another source of damage / другой источник урона.
      },
      // Color by vehicle type (macro {{c:vtype}}).
      // Цвет по типу техники (макрос {{c:vtype}}).
      "c:vtype": {
        "HT": "#FFACAC",         // heavy tank / тяжёлый танк.
        "MT": "#FFF198",         // medium tank / средний танк.
        "LT": "#A2FF9A",         // light tank / лёгкий танк.
        "TD": "#A0CFFF",         // tank destroyer / ПТ-САУ.
        "SPG": "#EFAEFF",        // SPG / САУ.
        "not_vehicle": "#CCCCCC" // another source of damage / другой источник урона.
      },
      // Hit kind (macro {{hit-effects}}).
      // Тип попадания (макрос {{hit-effects}}).
      "hit-effects": {
        "armor_pierced": "{{dmg}}",                                    // penetrated / пробито.
        "intermediate_ricochet": "{{l10n:intermediate_ricochet}}",     // ricochet (intermediate) / рикошет (промежуточный).
        "final_ricochet": "{{l10n:final_ricochet}}",                   // ricochet / рикошет.
        "armor_not_pierced": "{{l10n:armor_not_pierced}}",             // not penetrated / не пробито.
        "armor_pierced_no_damage": "{{l10n:armor_pierced_no_damage}}", // no damage / без урона.
        "unknown": "{{l10n:armor_pierced_no_damage}}"                  // unknown / неизвестно.
      },
      // Color by hit kind (macro {{c:hit-effects}}).
      // Цвет по типу попадания (макрос {{c:hit-effects}}).
      "c:hit-effects": {
        "armor_pierced": "#FF4D3C",           // penetrated (damage) / пробито (урон).
        "intermediate_ricochet": "#CCCCCC",   // ricochet (intermediate) / рикошет (промежуточный).
        "final_ricochet": "#CCCCCC",          // ricochet / рикошет.
        "armor_not_pierced": "#CCCCCC",       // not penetrated / не пробито.
        "armor_pierced_no_damage": "#CCCCCC", // no damage / без урона.
        "unknown": "#CCCCCC"                  // unknown / неизвестно.
      },
      // Critical hit (macro {{critical-hit}}).
      // Критическое попадание (макрос {{critical-hit}}).
      "critical-hit": {
        "critical": "*",  // critical hit / попадание с критическим повреждением.
        "no-critical": "" // without critical hit / попадание без критического повреждения.
      },
      // Damaged module or shell-shocked crew member (macro {{crit-device}}).
      // Поврежденный модуль или контуженный член экипажа (макрос {{crit-device}}).
      "crit-device": {
        "engine_crit": "{{l10n:engine}}",
        "ammo_bay_crit": "{{l10n:ammo_bay}}",
        "fuel_tank_crit": "{{l10n:fuel_tank}}",
        "radio_crit": "{{l10n:radio}}",
        "left_track_crit": "{{l10n:left_track}}",
        "right_track_crit": "{{l10n:right_track}}",
        "gun_crit": "{{l10n:gun}}",
        "turret_rotator_crit": "{{l10n:turret_rotator}}",
        "surveying_device_crit": "{{l10n:surveying_device}}",
        "engine_destr": "{{l10n:engine}}",
        "ammo_bay_destr": "{{l10n:ammo_bay}}",
        "fuel_tank_destr": "{{l10n:fuel_tank}}",
        "radio_destr": "{{l10n:radio}}",
        "left_track_destr": "{{l10n:left_track}}",
        "right_track_destr": "{{l10n:right_track}}",
        "gun_destr": "{{l10n:gun}}",
        "turret_rotator_destr": "{{l10n:turret_rotator}}",
        "surveying_device_destr": "{{l10n:surveying_device}}",
        "commander": "{{l10n:commander}}",
        "driver": "{{l10n:driver}}",
        "radioman": "{{l10n:radioman}}",
        "gunner": "{{l10n:gunner}}",
        "loader": "{{l10n:loader}}",
        "no-critical": ""
      },
      // Part of vehicle (macro {{comp-name}}).
      // Часть техники (макрос {{comp-name}}).
      "comp-name": {
        "turret": "{{l10n:turret}}",   // turret / башня.
        "hull": "{{l10n:hull}}",       // body / корпус.
        "chassis": "{{l10n:chassis}}", // suspension / ходовая.
        "wheel": "{{l10n:wheel}}",     // wheel / колесо.
        "gun": "{{l10n:gun}}",         // gun / орудие.
        "unknown": ""                  // unknown / неизвестно.
      },
      // Team attachment of the attacker (macro {{team-dmg}}).
      // Команданя принадлежность атакующего (макрос {{team-dmg}}).
      "team-dmg": {
        "ally-dmg": "",  // ally / союзник.
        "enemy-dmg": "", // enemy / противник.
        "player": "",    // self damage / урон по себе.
        "unknown": ""    // unknown / неизвестно.
      },
      // Color by team attachment of the attacker (macro {{c:team-dmg}}).
      // Цвет по командной принадлежности атакующего (макрос {{c:team-dmg}}).
      "c:team-dmg": {
        "ally-dmg": "#00EAFF",  // ally / союзник.
        "enemy-dmg": "#CCCCCC", // enemy / противник.
        "player": "#228855",    // self damage / урон по себе.
        "unknown": "#CCCCCC"    // unknown / неизвестно.
      },
      // Shell currency (macro {{costShell}}).
      // Валюта снаряда (макрос {{costShell}}).
      "costShell": {
        "gold-shell": "",   // gold / золото.
        "silver-shell": "", // credits / кредиты.
        "unknown": ""       // unknown / неизвестно.
      },
      // Color by shell currency (macro {{c:costShell}}).
      // Цвет по валюте снаряда (макрос {{c:costShell}}).
      "c:costShell": {
        "gold-shell": "#FFCC66",   // gold / золото.
        "silver-shell": "#CCCCCC", // credits / кредиты.
        "unknown": ""              // unknown / неизвестно.
      },
      // Shadow settings.
      // Настройки тени.
      "shadow": {
        "distance": 1,
        "angle": 90,
        "color": "#000000",
        "alpha": 75,
        "blur": 5,
        "strength": 3,
        "hideObject": false,
        "inner": false,
        "knockout": false,
        "quality": 1
      },
      // Damage log format.
      // Формат лога повреждений.
      "formatHistory": "<textformat tabstops='[30,130,165,180]'><font face='mono' size='12'>{{number%3d~.}}</font><tab><font color='{{c:dmg-kind}}'>{{dmg-kind}}</font><tab><font color='{{c:vtype}}'>{{vtype}}</font><tab><font color='{{c:team-dmg}}'>{{vehicle}}</font></textformat>"
    },
    // Background of the log of the received damage.
    // Подложка лога полученного урона.
    "logBackground": {
      "[dollar]ref": { "path":"damageLog.log" },
      // Damage log background format.
      // Формат подложки лога повреждений.
      "formatHistory": "<img height='20' width='310' src='xvm://res/icons/damageLog/{{dmg=0?no_dmg|dmg}}.png'>"
    },
    // Log of the received damage (alternative mode).
    // Лог полученного урона (альтернативный режим).
    "logAlt": {
      "[dollar]ref": { "path":"damageLog.log" },
      // true - show hits without damage.
      // true - отображать попадания без урона.
      "showHitNoDamage": true,
      // Damage log format.
      // Формат лога повреждений.
      "formatHistory": "<textformat tabstops='[30,130,165]'><font face='mono' size='12'>{{number%3d~.}}</font><tab><font color='{{c:dmg-kind}}'>{{dmg-kind}}</font><tab><font color='{{c:team-dmg}}'>{{name}}</font></textformat>"
    },
    // Background of the log of the received damage (alternative mode).
    // Подложка лога полученного урона (альтернативный режим).
    "logAltBackground": {
      "[dollar]ref": { "path":"damageLog.logBackground" },
      // Damage log background format.
      // Формат подложки лога повреждений.
      "formatHistory": "<img height='20' width='310' src='xvm://res/icons/damageLog/{{dmg=0?no_dmg|dmg}}.png'>"
    },
    // Last damage (hit).
    // Последний урон (попадание).
    "lastHit": {
      "[dollar]ref": { "path":"damageLog.log" },
      // true - allow to move last damage in battle and disallow macros for "x" and "y" settings.
      // false - disallow to move last damage in battle and allow macros for "x" and "y" settings.
      // true - разрешить перемещение последнего урона в бою и запретить макросы в настройках "x" и "y".
      // false - запретить перемещение последнего урона в бою и разрешить макросы в настройках "x" и "y".
      "moveInBattle": false,
      "x": -120,
      "y": 200,
      // true - show hits without damage.
      // true - отображать попадания без урона.
      "showHitNoDamage": true,
      // Display duration (seconds).
      // Продолжительность отображения (секунды).
      "timeDisplayLastHit": 7,
      // Shadow settings.
      // Настройки тени.
      "shadow": {
        "distance": 0,
        "blur": 6,
        "strength": 6,
        "color": "{{dmg=0?#000000|#770000}}"
      },
      // Kind of the received damage (macro {{dmg-kind}}).
      // Тип полученного урона (макрос {{dmg-kind}}).
      "dmg-kind": {
        "shot": "{{hit-effects}}",            // shot / попадание.
        "fire": "{{hit-effects}}",            // fire / пожар.
        "ramming": "{{hit-effects}}",         // ramming / таран.
        "world_collision": "{{hit-effects}}", // world collision / столкновение с объектами, падение.
        "drowning": "{{l10n:drowning}}",      // drowning / утопление.
        "overturn": "{{hit-effects}}",        // overturn / опрокидывание.
        "death_zone": "DZ",                   // death zone / смертельная зона.
        "gas_attack": "GA",                   // gas attack / газовая атака.
        "art_attack": "{{hit-effects}}",      // art attack / артиллерийская поддержка.
        "air_strike": "{{hit-effects}}"       // air strike / поддержка авиации.
      },
      // Last damage format.
      // Формат последнего урона.
      "formatLastHit": "<font size='36' color='{{c:dmg-kind}}'>{{dmg-kind}}</font>"
    }
  }
}