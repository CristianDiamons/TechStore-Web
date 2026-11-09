document.addEventListener("DOMContentLoaded", function () {

    const buscador = document.getElementById("buscador");
    const filtroCategoria = document.getElementById("filtroCategoria");
    const tarjetas = document.querySelectorAll(".tarjeta-producto");
    const contador = document.getElementById("contador");
    const mensajeVacio = document.getElementById("mensajeVacio");

    const abrirCarrito = document.getElementById("abrirCarrito");
    const cerrarCarrito = document.getElementById("cerrarCarrito");
    const seccionCarrito = document.getElementById("seccionCarrito");
    const listaCarrito = document.getElementById("listaCarrito");
    const carritoVacio = document.getElementById("carritoVacio");
    const cantidadCarrito = document.getElementById("cantidadCarrito");
    const totalCarrito = document.getElementById("totalCarrito");
    const vaciarCarrito = document.getElementById("vaciarCarrito");
    const botonComprar = document.getElementById("comprarCarrito");

    let carrito = [];

    try {
        const carritoGuardado = localStorage.getItem("techstore_carrito");

        if (carritoGuardado) {
            const datos = JSON.parse(carritoGuardado);

            if (Array.isArray(datos)) {
                carrito = datos.filter(function (producto) {
                    return producto &&
                        producto.id !== undefined &&
                        typeof producto.nombre === "string" &&
                        Number.isFinite(Number(producto.precio)) &&
                        Number.isInteger(Number(producto.cantidad)) &&
                        Number(producto.cantidad) > 0;
                }).map(function (producto) {
                    return {
                        id: String(producto.id),
                        nombre: producto.nombre,
                        precio: Number(producto.precio),
                        cantidad: Number(producto.cantidad)
                    };
                });
            }
        }
    } catch (error) {
        console.error("No se pudo recuperar el carrito:", error);
        carrito = [];
    }

    function guardarCarrito() {
        try {
            localStorage.setItem(
                "techstore_carrito",
                JSON.stringify(carrito)
            );
        } catch (error) {
            console.error("No se pudo guardar el carrito:", error);
        }
    }

    function filtrarProductos() {

        if (!buscador || !filtroCategoria) return;

        const texto = buscador.value.toLowerCase().trim();
        const categoria = filtroCategoria.value.toLowerCase();

        let visibles = 0;

        tarjetas.forEach(function (tarjeta) {

            const nombre = (tarjeta.dataset.nombre || "").toLowerCase();
            const categoriaProducto =
                (tarjeta.dataset.categoria || "").toLowerCase();

            const coincideNombre = nombre.includes(texto);

            const coincideCategoria =
                categoria === "" || categoriaProducto === categoria;

            const mostrar = coincideNombre && coincideCategoria;

            tarjeta.hidden = !mostrar;

            if (mostrar) {
                visibles++;
            }
        });

        if (contador) {
            contador.textContent = visibles + " productos encontrados";
        }

        if (mensajeVacio) {
            mensajeVacio.hidden = visibles !== 0;
        }
    }

    if (buscador) {
        buscador.addEventListener("input", filtrarProductos);
    }

    if (filtroCategoria) {
        filtroCategoria.addEventListener("change", filtrarProductos);
    }

    tarjetas.forEach(function (tarjeta) {

        if (tarjeta.querySelector(".boton-agregar")) return;

        const boton = document.createElement("button");

        boton.type = "button";
        boton.className = "boton-agregar";
        boton.textContent = "Agregar al carrito";

        boton.addEventListener("click", function () {

            agregarProducto({
                id: tarjeta.dataset.id,
                nombre: tarjeta.dataset.nombre,
                precio: Number(tarjeta.dataset.precio)
            });

        });

        tarjeta.appendChild(boton);
    });

    function agregarProducto(producto) {

        if (!producto.id ||
            !producto.nombre ||
            !Number.isFinite(producto.precio) ||
            producto.precio < 0) {
            return;
        }

        const existente = carrito.find(function (item) {
            return item.id === String(producto.id);
        });

        if (existente) {
            existente.cantidad++;
        } else {
            carrito.push({
                id: String(producto.id),
                nombre: producto.nombre,
                precio: producto.precio,
                cantidad: 1
            });
        }

        actualizarCarrito();
    }

    function actualizarContador() {

        const cantidadTotal = carrito.reduce(function (total, producto) {
            return total + producto.cantidad;
        }, 0);

        if (cantidadCarrito) {
            cantidadCarrito.textContent = cantidadTotal;
        }
    }

    function mostrarCarrito() {

        if (!listaCarrito) return;

        listaCarrito.replaceChildren();

        let total = 0;

        carrito.forEach(function (producto) {

            total += producto.precio * producto.cantidad;

            const elemento = document.createElement("div");
            elemento.className = "elemento-carrito";

            const informacion = document.createElement("div");
            informacion.className = "info-carrito";

            const nombre = document.createElement("h3");
            nombre.textContent = producto.nombre;

            const precio = document.createElement("p");
            precio.textContent = formatoMoneda(producto.precio);

            informacion.append(nombre, precio);

            const controles = document.createElement("div");
            controles.className = "controles-cantidad";

            const disminuir = document.createElement("button");
            disminuir.type = "button";
            disminuir.textContent = "−";
            disminuir.setAttribute("aria-label", "Disminuir cantidad");

            disminuir.addEventListener("click", function () {

                producto.cantidad--;

                if (producto.cantidad <= 0) {
                    eliminarProducto(producto.id);
                } else {
                    actualizarCarrito();
                }
            });

            const cantidad = document.createElement("span");
            cantidad.textContent = producto.cantidad;

            const aumentar = document.createElement("button");
            aumentar.type = "button";
            aumentar.textContent = "+";
            aumentar.setAttribute("aria-label", "Aumentar cantidad");

            aumentar.addEventListener("click", function () {
                producto.cantidad++;
                actualizarCarrito();
            });

            const eliminar = document.createElement("button");
            eliminar.type = "button";
            eliminar.className = "boton-eliminar";
            eliminar.textContent = "Eliminar";

            eliminar.addEventListener("click", function () {
                eliminarProducto(producto.id);
            });

            controles.append(
                disminuir,
                cantidad,
                aumentar,
                eliminar
            );

            elemento.append(informacion, controles);
            listaCarrito.appendChild(elemento);
        });

        if (totalCarrito) {
            totalCarrito.textContent = formatoMoneda(total);
        }

        if (carritoVacio) {
            carritoVacio.hidden = carrito.length !== 0;
        }

        if (vaciarCarrito) {
            vaciarCarrito.disabled = carrito.length === 0;
        }

        if (botonComprar && !botonComprar.dataset.procesando) {
            botonComprar.disabled = carrito.length === 0;
        }
    }

    function eliminarProducto(id) {

        carrito = carrito.filter(function (producto) {
            return producto.id !== id;
        });

        actualizarCarrito();
    }

    function actualizarCarrito() {

        guardarCarrito();
        mostrarCarrito();
        actualizarContador();
    }

    function formatoMoneda(valor) {

        return valor.toLocaleString("es-MX", {
            style: "currency",
            currency: "MXN"
        });
    }

    if (abrirCarrito && seccionCarrito) {

        abrirCarrito.addEventListener("click", function () {
            seccionCarrito.hidden = !seccionCarrito.hidden;
        });
    }

    if (cerrarCarrito && seccionCarrito) {

        cerrarCarrito.addEventListener("click", function () {
            seccionCarrito.hidden = true;
        });
    }

    if (vaciarCarrito) {

        vaciarCarrito.addEventListener("click", function () {

            carrito = [];
            actualizarCarrito();
        });
    }

    if (botonComprar) {

        botonComprar.addEventListener("click", async function () {

            if (carrito.length === 0) {
                alert("Tu carrito está vacío. Agrega productos antes de comprar.");
                return;
            }

            if (window.techStorePuedeComprar !== true) {

                alert(
                    "Para realizar una compra necesitas iniciar sesión " +
                    "con una cuenta de cliente."
                );

                window.location.href = window.techStoreLoginUrl;
                return;
            }

            const total = carrito.reduce(function (suma, producto) {
                return suma + producto.precio * producto.cantidad;
            }, 0);

            const confirmar = confirm(
                "¿Deseas confirmar tu compra?\n\n" +
                "Total aproximado: " + formatoMoneda(total)
            );

            if (!confirmar) return;

            const token = document.querySelector(
                '#tokenForm input[name="__RequestVerificationToken"]'
            )?.value;

            if (!token) {
                alert(
                    "No se pudo validar la solicitud. " +
                    "Recarga la página e inténtalo nuevamente."
                );
                return;
            }

            try {

                botonComprar.disabled = true;
                botonComprar.dataset.procesando = "true";
                botonComprar.textContent = "Procesando compra...";

                const respuesta = await fetch("/Compras/Registrar", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": token
                    },
                    body: JSON.stringify({
                        productos: carrito.map(function (producto) {
                            return {
                                idProducto: Number(producto.id),
                                cantidad: producto.cantidad
                            };
                        })
                    })
                });

                const resultado = await respuesta.json();

                if (!respuesta.ok || !resultado.correcto) {
                    throw new Error(
                        resultado.mensaje ||
                        "No se pudo registrar la compra."
                    );
                }

                alert(
                    "¡Gracias por tu compra!\n\n" +
                    resultado.mensaje + "\n" +
                    "Número de pedido: #" + resultado.idPedido +
                    "\nTotal registrado: " + formatoMoneda(total) +
                    "\n\n¡Esperamos verte pronto en TechStore!"
                );

                carrito = [];
                actualizarCarrito();

                if (seccionCarrito) {
                    seccionCarrito.hidden = true;
                }

            } catch (error) {

                console.error("Error al registrar la compra:", error);

                alert(
                    error.message ||
                    "Ocurrió un error al procesar tu compra. Inténtalo nuevamente."
                );

            } finally {

                delete botonComprar.dataset.procesando;
                botonComprar.textContent = "Comprar";
                botonComprar.disabled = carrito.length === 0;
            }
        });
    }

    mostrarCarrito();
    actualizarContador();

});
