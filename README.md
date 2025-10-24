Aquí realizamos nuestra primera versión en GitHub de Jugada Maestra. Cuenta con el Login con un funcionamiento lógico exitoso, y una clara visualización del tablero con las 3 entidades drill up - drill down obligatorias, con objetivos propuestos por deporte y gracias a ello, con un tablero con su semaforización exitosa. Además se realizó un prototipo para que el usuario pueda apostar, dándole un estilo de un proyecto de apuestas deportivas.

Su funcionamiento es el siguiente:

Al compilar el proyecto, se abrirá el login para que el usuario pueda loguearse. En caso de no estar registrado, el usuario completará el formulario poniendo su nombre y contraseña. El sistema automáticamente guardará esa contraseña hasheada, utilizando el BCrypt.

Una vez iniciado, podremos ver el tablero de 3 deportes distintos: fútbol, tenis y básquet. Con cada uno, se verá su detalle, el siguiente contará con un monto recaudado, un monto establecido como objetivo a cumplir, y un estado: Superado / cumple / no superado. Podremos ver en detalle, la cantidad recaudada por mes, y a la vez cuánto se recaudó por liga del deporte establecido. En base al monto establecido como objetivo, el mismo contará con su semaforización para las tablas de los meses y para las tablas de las ligas.

El monto establecido como objetivo a cumplir, se puede establecer al inicio de los tableros de los deportes.

Veamos un ejemplo práctico:
Para el deporte fútbol, establecemos un monto como objetivo a cumplir de $30.000. Si observamos su tablero, el monto recaudado es de $185.000, por lo tanto su estado será: "Objetivo Superado". Si vemos en detalle, en la tabla de "Meses" vemos que en Agosto se recaudó $100.000, Septiembre $30.000 y Octubre $55.000. El sistema de manera automática pondrá en verde el mes de Agosto, en rojo Septiembre y en azul (intermedio) el mes de Octubre. Y si entramos a la tabla de Ligas, dentro de la Serie A se recaudó $40.000, Premier League $60.000, y LaLiga $85.000. Por lo tanto rojo para Serie A, Premier League azul, y LaLiga en verde.

Aquí vemos un drill up y drill down de manera clara:
Deportes <--> Ligas <--> Meses

Por último, se realizó un botón para Apostar, pero es tan solo un prototipo.
