"use strict";
var __assign = (this && this.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
var TABLET = Tablet.getTablet("com.highfidelity.interface.tablet.system");
var flags = {
    message_debug: false,
    nametag_debug: false
};
var ACTIVE_ICON = Script.resolvePath("user-inside-bubble-speech-inv.svg");
var ICON = Script.resolvePath("user-inside-bubble-speech.svg");
var SETTINGS_PREFIX = "snail.customtags.";
var DEFAULTS = {
    "enabled": true,
    "seeSelf": true,
    "quickToggle": false,
    "allowCustom": true,
    "provideCustom": true,
    "bg": Script.resolvePath("bg.png"),
    "bgTalking": Script.resolvePath("bg-talking.png")
};
var DEFAULT_CONFIG = {
    "v": 1,
    "bg": Script.resolvePath("bg.png"),
    "bg_talking": Script.resolvePath("bg-talking.png")
};
function parseOr(data, def) {
    try {
        return JSON.parse(data);
    }
    catch (e) {
        return def;
    }
}
function dumpStack() {
    var f = arguments.callee;
    while (f) {
        console.log("------");
        console.log(f.toString());
        f = f.caller;
    }
}
var Event = (function () {
    function Event() {
        var _this = this;
        this.handlers = [];
        this.connect = function (handler) { return _this.handlers.push(handler); };
        this.disconnect = function (handler) { return _this.handlers = _this.handlers.filter(function (h) { return h != handler; }); };
        this.emit = function (data) { return _this.handlers.forEach(function (fn) { return fn(data); }); };
    }
    return Event;
}());
var Nametag = (function () {
    function Nametag(config, avatar) {
        var _a;
        var _this = this;
        this.updateAvatar = function (avatar) {
            var _a;
            var name = avatar.sessionDisplayName;
            var size;
            do {
                size = Overlays.textSize(_this.text, name);
                if (size.width <= 1)
                    break;
                name = name.substr(0, name.length - 1);
            } while (name.length > 0);
            var leftMargin = (1 - size.width) / 2;
            var topMargin = (.4 - size.height) / 2;
            var scale = avatar.scale;
            var textEdit = {
                text: name,
                leftMargin: leftMargin,
                topMargin: topMargin,
                lineHeight: .15,
                localRotation: Quat.IDENTITY
            };
            var pose = { x: 0, y: 1.25 * scale, z: 0 };
            var show_fg = avatar.audioLoudness == 0;
            Overlays.editOverlays((_a = {},
                _a[Uuid.toString(_this.background)] = { localPosition: pose, visible: show_fg, localRotation: Quat.IDENTITY },
                _a[Uuid.toString(_this.bg_talking)] = { localPosition: pose, visible: !show_fg, localRotation: Quat.IDENTITY },
                _a[Uuid.toString(_this.text)] = textEdit,
                _a[Uuid.toString(_this.text_background)] = textEdit,
                _a));
        };
        this.remove = function () {
            if (flags.nametag_debug)
                console.log("Removing nametag " + _this.avatar.sessionUUID);
            Overlays.deleteOverlay(_this.background);
            Overlays.deleteOverlay(_this.text);
            Overlays.deleteOverlay(_this.text_background);
            Overlays.deleteOverlay(_this.bg_talking);
        };
        this.avatar = avatar;
        if (flags.nametag_debug)
            console.log("Creating nametag " + avatar.sessionUUID);
        this.config = config;
        var common = {
            parentID: avatar.sessionUUID,
            dimensions: { x: 1, y: .25 },
            isFacingAvatar: true,
            alpha: 1,
            emissive: true,
            backgroundAlpha: 0,
            lineHeight: .15,
            keepAspectRatio: false
        };
        this.background = Overlays.addOverlay("image3d", __assign({}, common, { url: this.config.bg }));
        this.bg_talking = Overlays.addOverlay("image3d", __assign({}, common, { url: this.config.bg_talking }));
        this.text = Overlays.addOverlay("text3d", __assign({}, common, { parentID: this.background, color: { red: 0, green: 0, blue: 0 }, localPosition: { x: 0, y: 0, z: .01 } }));
        this.text_background = Overlays.addOverlay("text3d", __assign({}, common, { parentID: this.text, color: { red: 255, green: 255, blue: 255 }, localPosition: { x: -0.01, y: 0.01, z: .01 } }));
        Overlays.editOverlays((_a = {},
            _a[Uuid.toString(this.bg_talking)] = { url: config.bg_talking },
            _a[Uuid.toString(this.background)] = { url: config.bg },
            _a));
        Script.scriptEnding.connect(function () {
            console.log("Script ending unload");
            _this.remove();
        });
    }
    return Nametag;
}());
var Communications = new (function () {
    function class_1() {
        var _this = this;
        this.channel = "snailnametagschannel";
        this.onConfigReceived = new Event();
        this.onConfigRequested = new Event();
        this.onConnectionReady = new Event();
        this.connected = false;
        this.reconnect = function () {
            try {
                Messages.messageReceived.disconnect(_this.messageReceived);
            }
            catch (e) { }
            Messages.subscribe(_this.channel);
            Messages.messageReceived.connect(_this.messageReceived);
            _this.connected = false;
            _this.pingThread();
        };
        this.messageReceived = function (channel, message, sender, localOnly) {
            if (sender == null)
                return;
            var packet = parseOr(message, false);
            if (flags.message_debug)
                console.log("<<< " + sender + " " + JSON.stringify(packet, null, 2));
            if (!packet || !packet.method)
                return;
            if (packet.method == "ping" && MyAvatar.sessionUUID == sender) {
                _this.connected = true;
                _this.onConnectionReady.emit();
            }
            if (sender == MyAvatar.sessionUUID)
                return;
            switch (packet.method) {
                case "announce":
                    _this.onConfigReceived.emit({ sender: sender, config: packet.config });
                    break;
                case "request":
                    _this.onConfigRequested.emit();
                    break;
            }
        };
        this.pingThread = function () {
            if (_this.connected)
                return;
            _this.sendMessage({ "method": "ping" });
            Script.setTimeout(_this.pingThread, 1000);
        };
        Window.domainChanged.connect(this.reconnect);
        this.reconnect();
    }
    class_1.prototype.sendMessage = function (obj) {
        if (flags.message_debug)
            console.log(">>> " + JSON.stringify(obj, null, 2));
        Messages.sendMessage(this.channel, JSON.stringify(obj));
    };
    class_1.prototype.announceConfig = function (config) {
        this.sendMessage({
            "method": "announce",
            "config": config
        });
    };
    class_1.prototype.requestConfig = function () {
        this.sendMessage({ "method": "request" });
    };
    return class_1;
}());
new (function () {
    function class_2() {
        var _this = this;
        this.announceConfig = function () {
            if (_this.getSetting("provideCustom") && _this.getSetting("enabled"))
                Communications.announceConfig(_this.local_config);
        };
        this.receivedConfig = function (_a) {
            var sender = _a.sender, config = _a.config;
            if (!_this.getSetting("enabled"))
                return;
            if (!_this.getSetting("allowCustom"))
                return;
            var id = Uuid.toString(sender);
            var avatar = AvatarList.getAvatar(sender);
            if (_this.nametags[id]) {
                _this.nametags[id].remove();
            }
            _this.nametags[id] = new Nametag(config, avatar);
        };
        this.webEventReceived = function (data) {
            var req = parseOr(data, false);
            if (req === false)
                return;
            switch (req.name) {
                case "ready":
                    for (var key in DEFAULTS)
                        _this.emitScriptEvent({
                            id: key,
                            value: _this.getSetting(key)
                        });
                    break;
                case "set":
                    _this.putSetting(req.id, req.value);
                    break;
            }
        };
        this.nametags = {};
        this.updateLocalConfig();
        this._scheduled = false;
        this.button = TABLET.addButton({
            activeIcon: ACTIVE_ICON,
            icon: ICON,
            isActive: this.getSetting("enabled"),
            text: "Nametags"
        });
        Script.scriptEnding.connect(function () {
            Menu.removeMenu("View > Nametags");
            TABLET.removeButton(_this.button);
            TABLET.webEventReceived.disconnect(_this.webEventReceived);
        });
        this.button.clicked.connect(function () {
            if (_this.getSetting("quickToggle"))
                _this.toggle();
            else
                _this.openApp();
        });
        TABLET.webEventReceived.connect(this.webEventReceived);
        Menu.addMenu("View > Nametags");
        Menu.addMenuItem("View > Nametags", "Toggle Nametags");
        Menu.addMenuItem("View > Nametags", "Configure Nametags");
        Menu.menuItemEvent.connect(function (item) {
            switch (item) {
                case 'Toggle Nametags': return _this.toggle();
                case 'Configure Nametags': return _this.openApp();
            }
        });
        Communications.onConfigRequested.connect(this.announceConfig);
        Communications.onConfigReceived.connect(this.receivedConfig);
        Communications.onConnectionReady.connect(function () {
            _this.announceConfig();
            _this.requestConfigs();
        });
        this.update();
    }
    class_2.prototype.requestConfigs = function () {
        if (this.getSetting("allowCustom") && this.getSetting("enabled"))
            Communications.requestConfig();
    };
    class_2.prototype.openApp = function () { TABLET.gotoWebScreen(Script.resolvePath("tablet.html")); };
    class_2.prototype.toggle = function () { this.putSetting("enabled", !this.getSetting("enabled")); };
    class_2.prototype.emitScriptEvent = function (obj) { TABLET.emitScriptEvent(JSON.stringify(obj)); };
    class_2.prototype.getSetting = function (key) { return Settings.getValue(SETTINGS_PREFIX + key, DEFAULTS[key]); };
    class_2.prototype.putSetting = function (key, value) {
        Settings.setValue(SETTINGS_PREFIX + key, value);
        this.emitScriptEvent({ id: key, value: value });
        switch (key) {
            case "enabled":
                this.button.editProperties({ isActive: value });
                if (value) {
                    this.update();
                    this.requestConfigs();
                }
                else
                    this.removeTags();
                break;
            case "allowCustom":
                this.requestConfigs();
                break;
            case "bg":
            case "bgTalking":
                this.updateLocalConfig();
                this.announceConfig();
                break;
            case "provideCustom":
                this.announceConfig();
                break;
        }
    };
    class_2.prototype.updateLocalConfig = function () {
        this.local_config = {
            v: 1,
            bg: this.getSetting("bg"),
            bg_talking: this.getSetting("bgTalking")
        };
    };
    class_2.prototype.removeTags = function () { this.updateNametags({}); };
    class_2.prototype.update = function () {
        var _this = this;
        if (this._scheduled)
            return;
        if (!this.getSetting("enabled"))
            return;
        this._scheduled = true;
        Script.setTimeout(function () {
            _this._scheduled = false;
            _this.update();
        }, 250);
        var avatarMap = {};
        var seeSelf = this.getSetting("seeSelf");
        AvatarList.getAvatarIdentifiers()
            .forEach(function (uuid) {
            if (seeSelf)
                uuid = uuid ? uuid : MyAvatar.sessionUUID;
            if (!uuid)
                return;
            var avatar = AvatarList.getAvatar(uuid);
            if (avatar.sessionUUID == null) {
                console.log("Skipping null avatar?");
                return;
            }
            avatarMap[Uuid.toString(uuid)] = avatar;
        });
        this.updateNametags(avatarMap);
    };
    class_2.prototype.updateNametags = function (avatarMap) {
        var newNametags = {};
        for (var uuid in avatarMap) {
            var existingNametag = this.nametags[uuid];
            if (existingNametag) {
                newNametags[uuid] = this.nametags[uuid];
                delete this.nametags[uuid];
            }
            else {
                var config = DEFAULT_CONFIG;
                console.log("Update adding nametag " + uuid);
                if (uuid == Uuid.toString(MyAvatar.sessionUUID))
                    config = this.local_config;
                newNametags[uuid] = new Nametag(config, avatarMap[uuid]);
            }
            newNametags[uuid].updateAvatar(avatarMap[uuid]);
        }
        for (var uuid in this.nametags) {
            console.log("Update removing nametag " + uuid);
            this.nametags[uuid].remove();
        }
        this.nametags = newNametags;
    };
    return class_2;
}());
