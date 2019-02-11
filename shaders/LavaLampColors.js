// http://home.snail.rocks:8000/shaders/LavaLampColors.js
(function lavalamp() {
    this.clickDownOnEntity = function (entityID, mouseEvent) {
        var data = JSON.parse(Entities.getEntityProperties(entityID, ["userData"]).userData);
        uniforms = data["ProceduralEntity"]["uniforms"];
        uniforms["c1"] = [Math.random(), Math.random(), Math.random()];
        uniforms["c2"] = [Math.random(), Math.random(), Math.random()];
        Entities.editEntity(entityID, { userData: JSON.stringify(data, null, 2) });
    };
});
