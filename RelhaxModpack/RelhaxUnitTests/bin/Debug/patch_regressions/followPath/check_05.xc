﻿{
  "def": {
    "enemySpottedMarker": {
      "enabled": true,
      "alpha": "{{a:spotted}}",
      "x": 88,
      "y": -2,
      "align": "center",
      "bindToIcon": true,
      "format": "<font color='{{c:spotted}}'>{{spotted}}</font>",
      "shadow": {}
    },
    "xmqpServiceMarker": {
      "enabled": true,
      "x": 88,
      "y": -2,
      "align": "center",
      "bindToIcon": true,
      "textFormat": {
        "font": "xvm",
        "size": 24
      },
      "format": "<font color='{{alive?{{x-spotted?#FFBB00|{{x-sense-on?#D9D9D9|#BFBFBF}}}}|#FFFFFF}}' alpha='{{alive?#FF|#80}}'>{{alive?{{x-spotted?&#x70;|{{x-sense-on?&#x70;|{{x-enabled?&#x7A;}}}}}}}}</font>",
      "shadow": {}
    },
    "clanIcon": {
      "enabled": true,
      "x": 65,
      "y": 6,
      "width": 16,
      "height": 16,
      "align": "center",
      "alpha": 90,
      "bindToIcon": true,
      "src": "{{clanicon}}"
    },
    "xvmUserMarker": {
      "enabled": false,
      "x": 10,
      "y": 5,
      "bindToIcon": true,
      "src": "xvm://res/icons/xvm/xvm-user-{{xvm-user|none}}.png"
    },
    "hpBarBg": {
      "hotKeyCode": 56,
      "onHold": "true",
      "visibleOnHotKey": true,
      "x": 96,
      "y": 6,
      "width": 72,
      "bindToIcon": true,
      "height": 14,
      "bgColor": "0x000000",
      "alpha": "{{alive?35|0}}"
    },
    "hpBar": {
      "hotKeyCode": 56,
      "onHold": "true",
      "visibleOnHotKey": true,
      "x": 97,
      "y": 7,
      "bindToIcon": true,
      "width": "{{hp-ratio:70}}",
      "height": 12,
      "bgColor": "{{player?#FFDD33|{{c:system}}}}",
      "alpha": "{{alive?50|0}}"
    },
    "hp": {
      "hotKeyCode": 56,
      "onHold": "true",
      "visibleOnHotKey": true,
      "bindToIcon": true,
      "alpha": "{{alive?100|0}}",
      "x": 96,
      "width": 72,
      "y": 4,
      "textFormat": {
        "font": "$FieldFont",
        "size": 11,
        "color": "0xD9D9D9",
        "bold": "true",
        "align": "center"
      },
      "format": "<font alpha='{{alive?{{ready?#FF|#80}}|#80}}'>{{alive?{{hp|{{l10n:No data}}}}|{{l10n:Destroyed}}}}</font>",
      "shadow": {
        "enabled": true,
        "color": "0x000000",
        "alpha": 100,
        "blur": 4,
        "strength": 1,
        "distance": 0,
        "angle": 0
      }
    },
    "newDef": {}
  },
  "playersPanel": {
    "enabled": true,
    "alpha": 80,
    "iconAlpha": 100,
    "removeSelectedBackground": false,
    "removePanelsModeSwitcher": false,
    "startMode": "large",
    "altMode": null,
    "none": {
      "enabled": true,
      "expandAreaWidth": 230,
      "layout": "vertical",
      "fixedPosition": false,
      "inviteIndicatorAlpha": 100,
      "inviteIndicatorX": 0,
      "inviteIndicatorY": 0,
      "extraFields": {
        "leftPanel": {
          "x": 0,
          "y": 65,
          "width": 350,
          "height": 25,
          "formats": []
        },
        "rightPanel": {
          "x": 0,
          "y": 65,
          "width": 350,
          "height": 25,
          "formats": []
        }
      }
    },
    "short": {
      "enabled": true,
      "standardFields": [
        "frags"
      ],
      "expandAreaWidth": 230,
      "removeSquadIcon": false,
      "squadIconAlpha": 100,
      "vehicleIconOffsetXLeft": 0,
      "vehicleIconOffsetXRight": 0,
      "vehicleLevelOffsetXLeft": 0,
      "vehicleLevelOffsetXRight": 0,
      "vehicleLevelAlpha": 100,
      "fragsOffsetXLeft": 0,
      "fragsOffsetXRight": 0,
      "fragsWidth": 24,
      "fragsFormatLeft": "{{frags}}",
      "fragsFormatRight": "{{frags}}",
      "fragsShadowLeft": null,
      "fragsShadowRight": null,
      "rankBadgeOffsetXLeft": 0,
      "rankBadgeOffsetXRight": 0,
      "rankBadgeWidth": 24,
      "rankBadgeAlpha": "{{alive?100|70}}",
      "nickOffsetXLeft": 0,
      "nickOffsetXRight": 0,
      "nickMinWidth": 46,
      "nickMaxWidth": 158,
      "nickFormatLeft": "<font face='mono' size='{{xvm-stat?13|0}}' color='{{c:xr}}' alpha='{{alive?#FF|#80}}'>{{r}}</font> {{name%.15s~..}}<font alpha='#A0'>{{clan}}</font>",
      "nickFormatRight": "<font alpha='#A0'>{{clan}}</font>{{name%.15s~..}} <font face='mono' size='{{xvm-stat?13|0}}' color='{{c:xr}}' alpha='{{alive?#FF|#80}}'>{{r}}</font>",
      "nickShadowLeft": null,
      "nickShadowRight": null,
      "vehicleOffsetXLeft": 0,
      "vehicleOffsetXRight": 0,
      "vehicleWidth": 72,
      "vehicleFormatLeft": "{{vehicle}}",
      "vehicleFormatRight": "{{vehicle}}",
      "vehicleShadowLeft": null,
      "vehicleShadowRight": null,
      "fixedPosition": false,
      "extraFieldsLeft": [
        ${"def.hpBarBg"},
        ${"def.hpBar"},
        ${"def.hp"},
        ${"def.clanIcon"},
        ${"def.xvmUserMarker"},
        ${"def.xmqpServiceMarker"}
      ],
      "extraFieldsRight": [
        ${"def.hpBarBg"},
        ${"def.hpBar"},
        ${"def.hp"},
        ${"def.clanIcon"},
        ${"def.xvmUserMarker"},
        ${"def.enemySpottedMarker"}
      ]
    },
    "medium": {
      "enabled": true,
      "standardFields": [
        "frags",
        "badge",
        "nick"
      ],
      "expandAreaWidth": 230,
      "removeSquadIcon": false,
      "squadIconAlpha": 100,
      "vehicleIconOffsetXLeft": 0,
      "vehicleIconOffsetXRight": 0,
      "vehicleLevelOffsetXLeft": 0,
      "vehicleLevelOffsetXRight": 0,
      "vehicleLevelAlpha": 100,
      "fragsOffsetXLeft": 0,
      "fragsOffsetXRight": 0,
      "fragsWidth": 24,
      "fragsFormatLeft": "{{frags}}",
      "fragsFormatRight": "{{frags}}",
      "fragsShadowLeft": null,
      "fragsShadowRight": null,
      "rankBadgeOffsetXLeft": 0,
      "rankBadgeOffsetXRight": 0,
      "rankBadgeWidth": 24,
      "rankBadgeAlpha": "{{alive?100|70}}",
      "nickOffsetXLeft": 0,
      "nickOffsetXRight": 0,
      "nickMinWidth": 46,
      "nickMaxWidth": 158,
      "nickFormatLeft": "<font color='{{c:xr}}' alpha='{{alive?#FF|#80}}'>{{name%.12s~..}}</font> <font alpha='#A0'>{{clan}}</font>",
      "nickFormatRight": "<font alpha='#A0'>{{clan}}</font> <font color='{{c:xr}}' alpha='{{alive?#FF|#80}}'>{{name%.12s~..}}</font>",
      "nickShadowLeft": null,
      "nickShadowRight": null,
      "vehicleOffsetXLeft": 0,
      "vehicleOffsetXRight": 0,
      "vehicleWidth": 72,
      "vehicleFormatLeft": "<font color='{{c:xr}}' alpha='{{alive?#FF|#80}}'>{{vehicle}}</font>",
      "vehicleFormatRight": "<font color='{{c:xr}}' alpha='{{alive?#FF|#80}}'>{{vehicle}}</font>",
      "vehicleShadowLeft": null,
      "vehicleShadowRight": null,
      "fixedPosition": false,
      "extraFieldsLeft": [
        ${"def.hpBarBg"},
        ${"def.hpBar"},
        ${"def.hp"},
        ${"def.clanIcon"},
        ${"def.xvmUserMarker"},
        ${"def.xmqpServiceMarker"}
      ],
      "extraFieldsRight": [
        ${"def.hpBarBg"},
        ${"def.hpBar"},
        ${"def.hp"},
        ${"def.clanIcon"},
        ${"def.xvmUserMarker"},
        ${"def.enemySpottedMarker"}
      ]
    },
    "medium2": {
      "enabled": true,
      "standardFields": [
        "frags",
        "vehicle"
      ],
      "expandAreaWidth": 230,
      "removeSquadIcon": false,
      "squadIconAlpha": 100,
      "vehicleIconOffsetXLeft": 0,
      "vehicleIconOffsetXRight": 0,
      "vehicleLevelOffsetXLeft": 0,
      "vehicleLevelOffsetXRight": 0,
      "vehicleLevelAlpha": 100,
      "fragsOffsetXLeft": 0,
      "fragsOffsetXRight": 0,
      "fragsWidth": 24,
      "fragsFormatLeft": "{{frags}}",
      "fragsFormatRight": "{{frags}}",
      "fragsShadowLeft": null,
      "fragsShadowRight": null,
      "rankBadgeOffsetXLeft": 0,
      "rankBadgeOffsetXRight": 0,
      "rankBadgeWidth": 24,
      "rankBadgeAlpha": "{{alive?100|70}}",
      "nickOffsetXLeft": 0,
      "nickOffsetXRight": 0,
      "nickMinWidth": 46,
      "nickMaxWidth": 158,
      "nickFormatLeft": "<font color='{{c:xr}}' alpha='{{alive?#FF|#80}}'>{{name%.12s~..}}</font> <font alpha='#A0'>{{clan}}</font>",
      "nickFormatRight": "<font alpha='#A0'>{{clan}}</font> <font color='{{c:xr}}' alpha='{{alive?#FF|#80}}'>{{name%.12s~..}}</font>",
      "nickShadowLeft": null,
      "nickShadowRight": null,
      "vehicleOffsetXLeft": 0,
      "vehicleOffsetXRight": 0,
      "vehicleWidth": 72,
      "vehicleFormatLeft": "<font color='{{c:xr}}' alpha='{{alive?#FF|#80}}'>{{vehicle}}</font>",
      "vehicleFormatRight": "<font color='{{c:xr}}' alpha='{{alive?#FF|#80}}'>{{vehicle}}</font>",
      "vehicleShadowLeft": null,
      "vehicleShadowRight": null,
      "fixedPosition": false,
      "extraFieldsLeft": [
        ${"def.hpBarBg"},
        ${"def.hpBar"},
        ${"def.hp"},
        ${"def.clanIcon"},
        ${"def.xvmUserMarker"},
        ${"def.xmqpServiceMarker"}
      ],
      "extraFieldsRight": [
        ${"def.hpBarBg"},
        ${"def.hpBar"},
        ${"def.hp"},
        ${"def.clanIcon"},
        ${"def.xvmUserMarker"},
        ${"def.enemySpottedMarker"}
      ]
    },
    "large": {
      "enabled": true,
      "standardFields": [
        "frags",
        "badge",
        "nick",
        "vehicle"
      ],
      "removeSquadIcon": false,
      "squadIconAlpha": 100,
      "vehicleIconOffsetXLeft": 0,
      "vehicleIconOffsetXRight": 0,
      "vehicleLevelOffsetXLeft": 0,
      "vehicleLevelOffsetXRight": 0,
      "vehicleLevelAlpha": 100,
      "fragsOffsetXLeft": 0,
      "fragsOffsetXRight": 0,
      "fragsWidth": 24,
      "fragsFormatLeft": "{{frags}}",
      "fragsFormatRight": "{{frags}}",
      "fragsShadowLeft": null,
      "fragsShadowRight": null,
      "rankBadgeOffsetXLeft": 0,
      "rankBadgeOffsetXRight": 0,
      "rankBadgeWidth": 24,
      "rankBadgeAlpha": "{{alive?100|70}}",
      "nickOffsetXLeft": 0,
      "nickOffsetXRight": 0,
      "nickMinWidth": 46,
      "nickMaxWidth": 158,
      "nickFormatLeft": "<font face='mono' size='{{xvm-stat?13|0}}' color='{{c:xr}}' alpha='{{alive?#FF|#80}}'>{{r|--}}</font> {{name%.{{xvm-stat?{{r_size>2?10|13}}|15}}s~..}}<font alpha='#A0'>{{clan}}</font>",
      "nickFormatRight": "<font alpha='#A0'>{{clan}}</font>{{name%.{{xvm-stat?{{r_size>2?10|13}}|15}}s~..}} <font face='mono' size='{{xvm-stat?13|0}}' color='{{c:xr}}' alpha='{{alive?#FF|#80}}'>{{r}}</font>",
      "nickShadowLeft": null,
      "nickShadowRight": null,
      "vehicleOffsetXLeft": 0,
      "vehicleOffsetXRight": 0,
      "vehicleWidth": 72,
      "vehicleFormatLeft": "{{vehicle}}",
      "vehicleFormatRight": "{{vehicle}}",
      "vehicleShadowLeft": null,
      "vehicleShadowRight": null,
      "fixedPosition": false,
      "extraFieldsLeft": [
        ${"def.hpBarBg"},
        ${"def.hpBar"},
        ${"def.hp"},
        ${"def.clanIcon"},
        ${"def.xvmUserMarker"},
        ${"def.xmqpServiceMarker"}
      ],
      "extraFieldsRight": [
        ${"def.hpBarBg"},
        ${"def.hpBar"},
        ${"def.hp"},
        ${"def.clanIcon"},
        ${"def.xvmUserMarker"},
        ${"def.enemySpottedMarker"}
      ]
    }
  }
}
