const TABLET = Tablet.getTablet("com.highfidelity.interface.tablet.system");
/*
Last release - http://cdn.snail.rocks/scripting/customTags/tags.js
Active dev - http://home.snail.rocks:8000/scripting/customTags/_tags.js

Some TODOs
    aspect ratio
    slightly larger name tags
    configurable font size
    disable talking vs not talking, just show bg texture
    turn text off

    friends / connections /strangers should be constraints for showing nametags
    gifs - This is gonna take a sprite sheet/what not.

    some suggested defaults 
    default for other people shouldn't be your own

    fix cache problems
    view > nametags > configure nametags is apparently obscure

    gravatar based images
    store configs

    clicking on people to toggle their nametags
    
next config should be something like:
{
    avatarState: [
        {imageUrl, subImage} // frames
    ]
}
    states?:
        default
        talking   
        afk
        typing
        create menu open

*/

const flags = {
    message_debug: false,
    nametag_debug: false
};

type Config = {
    "v": 1,
    "bg": string,
    "bg_talking": string
};

const ACTIVE_ICON = Script.resolvePath("user-inside-bubble-speech-inv.svg");
const ICON = Script.resolvePath("user-inside-bubble-speech.svg");

type SettingKey = "enabled" | "seeSelf" | "quickToggle" | "allowCustom" | "provideCustom" | "bg" | "bgTalking";
const SETTINGS_PREFIX = "snail.customtags.";

const DEFAULTS = {
    "enabled": true,
    "seeSelf": true,
    "quickToggle": false,
    "allowCustom": true,
    "provideCustom": true,
    "bg": Script.resolvePath("bg.png"),
    "bgTalking": Script.resolvePath("bg-talking.png")
};
const DEFAULT_CONFIG: Config = {
    "v": 1,
    "bg": Script.resolvePath("bg.png"),
    "bg_talking": Script.resolvePath("bg-talking.png")
};
function parseOr(data: string, def: any) {
    try {
        return JSON.parse(data);
    } catch (e) {
        return def
    }
}
function dumpStack() {
    let f = arguments.callee;
    while (f) {
        console.log("------");
        console.log(f.toString());
        f = f.caller;
    }
}

class Event<T> {
    private handlers: ((data: T) => void)[] = []
    connect = (handler: (data: T) => void) => this.handlers.push(handler);
    disconnect = (handler: (data: T) => void) => this.handlers = this.handlers.filter((h) => h != handler);
    emit = (data: T) => this.handlers.forEach((fn) => fn(data));
}

class Nametag {

    background: Uuid;
    text: Uuid;
    text_background: Uuid;
    bg_talking: Uuid;
    config: Config;
    avatar: Avatar;

    constructor(config: Config, avatar: Avatar) {
        this.avatar = avatar;
        if (flags.nametag_debug) console.log("Creating nametag " + avatar.sessionUUID);// + "\n" + JSON.stringify(config, null, 2));
        this.config = config;
        let common: any = {
            parentID: avatar.sessionUUID,
            dimensions: { x: 1, y: .25 },
            isFacingAvatar: true,
            alpha: 1,
            emissive: true,
            backgroundAlpha: 0,
            lineHeight: .15,
            keepAspectRatio: false
        };

        this.background = Overlays.addOverlay("image3d", { ...common, url: this.config.bg });
        this.bg_talking = Overlays.addOverlay("image3d", { ...common, url: this.config.bg_talking });
        this.text = Overlays.addOverlay("text3d", {
            ...common,
            parentID: this.background,
            color: { red: 0, green: 0, blue: 0 },
            localPosition: { x: 0, y: 0, z: .01 },
        });
        this.text_background = Overlays.addOverlay("text3d", {
            ...common,
            parentID: this.text,
            color: { red: 255, green: 255, blue: 255 },
            localPosition: { x: -0.01, y: 0.01, z: .01 },
        });

        Overlays.editOverlays({
            [Uuid.toString(this.bg_talking)]: { url: config.bg_talking },
            [Uuid.toString(this.background)]: { url: config.bg }
        });

        Script.scriptEnding.connect(() => {
            console.log("Script ending unload");
            this.remove();
        });
    }


    updateAvatar = (avatar: Avatar) => {
        let name = avatar.sessionDisplayName;
        let size;
        do {
            size = Overlays.textSize(this.text, name);
            if (size.width <= 1) break;
            name = name.substr(0, name.length - 1);
        } while (name.length > 0);
        let leftMargin = (1 - size.width) / 2;
        let topMargin = (.4 - size.height) / 2;

        const scale = avatar.scale;
        const textEdit = {
            text: name,
            leftMargin: leftMargin,
            topMargin: topMargin,
            lineHeight: .15,
            localRotation: Quat.IDENTITY
        };

        let pose = { x: 0, y: 1.25 * scale, z: 0 };
        const show_fg = avatar.audioLoudness == 0;
        Overlays.editOverlays({
            [Uuid.toString(this.background)]: { localPosition: pose, visible: show_fg, localRotation: Quat.IDENTITY },
            [Uuid.toString(this.bg_talking)]: { localPosition: pose, visible: !show_fg, localRotation: Quat.IDENTITY },
            [Uuid.toString(this.text)]: textEdit,
            [Uuid.toString(this.text_background)]: textEdit,
        });
    }

    remove = () => {
        if (flags.nametag_debug) console.log("Removing nametag " + this.avatar.sessionUUID);
        Overlays.deleteOverlay(this.background);
        Overlays.deleteOverlay(this.text);
        Overlays.deleteOverlay(this.text_background);
        Overlays.deleteOverlay(this.bg_talking);
    }
}

const Communications = new class {
    private readonly channel = "snailnametagschannel";
    readonly onConfigReceived = new Event<{ sender: Uuid, config: Config }>();
    readonly onConfigRequested = new Event<void>();
    readonly onConnectionReady = new Event<void>();
    private connected = false;
    constructor() {
        Window.domainChanged.connect(this.reconnect);
        this.reconnect();
    }
    private reconnect = () => {
        try {
            Messages.messageReceived.disconnect(this.messageReceived);
        } catch (e) { }
        Messages.subscribe(this.channel);
        Messages.messageReceived.connect(this.messageReceived);

        this.connected = false;
        this.pingThread();
    }
    private messageReceived = (channel: string, message: string, sender: Uuid, localOnly: boolean) => {
        if (sender == null)
            return;
        let packet = parseOr(message, false);
        if (flags.message_debug) console.log("<<< " + sender + " " + JSON.stringify(packet, null, 2));
        if (!packet || !packet.method) return;


        if (packet.method == "ping" && MyAvatar.sessionUUID == sender) {
            this.connected = true;
            this.onConnectionReady.emit();
        }

        if (sender == MyAvatar.sessionUUID)
            return;

        switch (packet.method) {
            case "announce":
                this.onConfigReceived.emit({ sender: sender, config: packet.config });
                break
            case "request":
                this.onConfigRequested.emit();
                break
        }
    }
    private sendMessage(obj: Object) {
        if (flags.message_debug) console.log(">>> " + JSON.stringify(obj, null, 2));
        Messages.sendMessage(this.channel, JSON.stringify(obj));
    }

    private pingThread = () => {
        if (this.connected) return;
        this.sendMessage({ "method": "ping" });
        Script.setTimeout(this.pingThread, 1000)
    }

    announceConfig(config: Config) {
        //dumpStack();
        this.sendMessage({
            "method": "announce",
            "config": config
        });
    }
    requestConfig() {
        this.sendMessage({ "method": "request" });
    }
};


new class {
    button: TabletButtonProxy
    _scheduled: boolean
    nametags: Map<Nametag>
    local_config!: Config

    constructor() {
        this.nametags = {};
        this.updateLocalConfig();
        this._scheduled = false;
        this.button = TABLET.addButton({
            activeIcon: ACTIVE_ICON,
            icon: ICON,
            isActive: this.getSetting("enabled"),
            text: "Nametags"
        });

        Script.scriptEnding.connect(() => {
            Menu.removeMenu("View > Nametags");
            TABLET.removeButton(this.button);
            TABLET.webEventReceived.disconnect(this.webEventReceived);
        });

        this.button.clicked.connect(() => {
            if (this.getSetting("quickToggle"))
                this.toggle();
            else
                this.openApp();
        });
        TABLET.webEventReceived.connect(this.webEventReceived);

        Menu.addMenu("View > Nametags");
        Menu.addMenuItem("View > Nametags", "Toggle Nametags");
        Menu.addMenuItem("View > Nametags", "Configure Nametags");
        Menu.menuItemEvent.connect((item) => {
            switch (item) {
                case 'Toggle Nametags': return this.toggle();
                case 'Configure Nametags': return this.openApp();
            }
        });

        Communications.onConfigRequested.connect(this.announceConfig);
        Communications.onConfigReceived.connect(this.receivedConfig);
        Communications.onConnectionReady.connect(() => {
            this.announceConfig();
            this.requestConfigs();
        });

        // Kick off the first update. 
        this.update();
    }

    announceConfig = () => {
        if (this.getSetting("provideCustom") && this.getSetting("enabled"))
            Communications.announceConfig(this.local_config);
    }

    requestConfigs() {
        if (this.getSetting("allowCustom") && this.getSetting("enabled"))
            Communications.requestConfig();
    }

    receivedConfig = ({ sender, config }: { sender: Uuid, config: Config }) => {
        if (!this.getSetting("enabled")) return;
        if (!this.getSetting("allowCustom")) return;

        const id = Uuid.toString(sender);
        const avatar = AvatarList.getAvatar(sender);

        if (this.nametags[id]) {
            this.nametags[id].remove();
        }

        this.nametags[id] = new Nametag(config, avatar);
    }

    webEventReceived = (data: string) => {
        const req = parseOr(data, false);
        if (req === false) return;
        switch (req.name) {
            case "ready":
                for (let key in DEFAULTS)
                    this.emitScriptEvent({
                        id: key,
                        value: this.getSetting(key as SettingKey)
                    });
                break;
            case "set":
                this.putSetting(req.id, req.value);
                break;
        }
    }

    openApp() { TABLET.gotoWebScreen(Script.resolvePath("tablet.html")); }
    toggle() { this.putSetting("enabled", !this.getSetting("enabled")); }
    emitScriptEvent(obj: Object) { TABLET.emitScriptEvent(JSON.stringify(obj)); }
    getSetting(key: SettingKey) { return Settings.getValue(SETTINGS_PREFIX + key, DEFAULTS[key]); }
    putSetting(key: SettingKey, value: string | boolean) {
        Settings.setValue(SETTINGS_PREFIX + key, value);
        this.emitScriptEvent({ id: key, value: value });

        // Handle updated settings
        switch (key) {
            case "enabled":
                this.button.editProperties({ isActive: value as boolean });
                if (value) {
                    this.update();
                    this.requestConfigs();
                } else
                    this.removeTags();
                break;

            case "allowCustom":
                this.requestConfigs();
                break;

            case "bg": case "bgTalking":
                this.updateLocalConfig();
                this.announceConfig();
                break;

            case "provideCustom":
                this.announceConfig();
                break;
        }
    }

    private updateLocalConfig() {
        this.local_config = {
            v: 1,
            bg: this.getSetting("bg") as string,
            bg_talking: this.getSetting("bgTalking") as string
        };
    }

    private removeTags() { this.updateNametags({}); }
    private update() {
        // If we have another call scheduled (via setTimeout) ignore this call.
        if (this._scheduled) return;

        // If we're nolonger enabled, bail.
        if (!this.getSetting("enabled")) return;

        // Okay we're gonna update, go ahead and schedule another update in the future.
        this._scheduled = true;
        Script.setTimeout(() => {
            this._scheduled = false;
            this.update();
        }, 250);

        let avatarMap: Map<Avatar> = {};
        let seeSelf = this.getSetting("seeSelf");
        AvatarList.getAvatarIdentifiers()
            .forEach((uuid) => {
                if (seeSelf)
                    uuid = uuid ? uuid : MyAvatar.sessionUUID;
                if (!uuid) return;

                const avatar = AvatarList.getAvatar(uuid);
                if (avatar.sessionUUID == null) {
                    console.log("Skipping null avatar?");
                    return;
                }

                avatarMap[Uuid.toString(uuid)] = avatar;
            });
        this.updateNametags(avatarMap);
    }

    private updateNametags(avatarMap: Map<Avatar>) {
        let newNametags: Map<Nametag> = {};

        for (let uuid in avatarMap) {
            // Get or create the nametag for this avatar.
            let existingNametag = this.nametags[uuid];
            if (existingNametag) {
                newNametags[uuid] = this.nametags[uuid];
                // We don't want to remove this nametag in a moment, remove it from the map 
                delete this.nametags[uuid];
            } else {
                let config = DEFAULT_CONFIG;
                console.log("Update adding nametag " + uuid);
                if (uuid == Uuid.toString(MyAvatar.sessionUUID))
                    config = this.local_config;
                newNametags[uuid] = new Nametag(config, avatarMap[uuid]);
            }

            // Update the avatar (notably after the config)
            newNametags[uuid].updateAvatar(avatarMap[uuid]);
        }

        // The remaining nametags weren't in the avatar map, remove them
        for (let uuid in this.nametags) {
            console.log("Update removing nametag " + uuid);
            this.nametags[uuid].remove();
        }
        this.nametags = newNametags;
    }
}