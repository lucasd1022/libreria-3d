const app = require("./app");
const config = require("./config");

app.listen(config.port, () => {
  console.log(`${config.tienda.nombre} escuchando en http://localhost:${config.port}`);
});
