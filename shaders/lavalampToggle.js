// http://home.snail.rocks:8000/shaders/lavalampToggle.js?1
(function myLib() {
    function stack(){
        console.log("-----");
        var f = arguments.callee;
        while(f) {
            console.log(f.toString().split("(")[0])
            f = f.caller;
        }
    }
    this.clickDownOnEntity = function (entityID, mouseEvent) {
        stack();
        console.log("B");
        var data = JSON.parse(Entities.getEntityProperties(entityID, ["userData"]).userData);
        uniforms = data["ProceduralEntity"]["uniforms"];
        uniforms["color"] = [Math.random(), Math.random(), Math.random()];
        Entities.editEntity(entityID, { userData: JSON.stringify(data, null, 2) });
    };
});
