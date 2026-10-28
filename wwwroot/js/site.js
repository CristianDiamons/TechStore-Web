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

    const carrito = [];

    function filtrarProductos() {
        const texto = buscador.value.toLowerCase().trim();
        const categoria = filtroCategoria.value.toLowerCase();
        let visibles = 0;

        tarjetas.forEach(function (tarjeta) {
            const nombre = tarjeta.dataset.nombre.toLowerCase();
            const categoriaProducto = tarjeta.dataset.categoria.toLowerCase();

            const coincideNombre = nombre.includes(texto);
            const coincideCategoria =
                categoria === "" || categoriaProducto === categoria;

            const mostrar = coincideNombre && coincideCategoria;

            tarjeta.hidden = !mostrar;

            if (mostrar) {
                visibles++;
            }
        });

        contador.textContent = visibles + " productos encontrados";
        mensajeVacio.hidden = visibles !== 0;
    }

    buscador.addEventListener("input", filtrarProductos);
    filtroCategoria.addEventListener("change", filtrarProductos);

    tarjetas.forEach(function (tarjeta) {
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
        const existente = carrito.find(function (item) {
            return item.id === producto.id;
        });

        if (existente) {
            existente.cantidad++;
        } else {
            carrito.push({
                ...producto,
                cantidad: 1
            });
        }

        mostrarCarrito();
    }

    function mostrarCarrito() {
        listaCarrito.replaceChildren();

        let cantidadTotal = 0;
        let total = 0;

        carrito.forEach(function (producto) {
            cantidadTotal += producto.cantidad;
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
                    const indice = carrito.indexOf(producto);
                    carrito.splice(indice, 1);
                }

                mostrarCarrito();
            });

            const cantidad = document.createElement("span");
            cantidad.textContent = producto.cantidad;

            const aumentar = document.createElement("button");
            aumentar.type = "button";
            aumentar.textContent = "+";
            aumentar.setAttribute("aria-label", "Aumentar cantidad");

            aumentar.addEventListener("click", function () {
                producto.cantidad++;
                mostrarCarrito();
            });

            const eliminar = document.createElement("button");
            eliminar.type = "button";
            eliminar.className = "boton-eliminar";
            eliminar.textContent = "Eliminar";

            eliminar.addEventListener("click", function () {
                const indice = carrito.indexOf(producto);
                carrito.splice(indice, 1);
                mostrarCarrito();
            });

            controles.append(disminuir, cantidad, aumentar, eliminar);
            elemento.append(informacion, controles);
            listaCarrito.appendChild(elemento);
        });

        cantidadCarrito.textContent =
            cantidadTotal + (cantidadTotal === 1
                ? " producto en el carrito"
                : " productos en el carrito");

        totalCarrito.textContent = formatoMoneda(total);
        carritoVacio.hidden = carrito.length !== 0;
        vaciarCarrito.disabled = carrito.length === 0;
    }

    function formatoMoneda(valor) {
        return valor.toLocaleString("es-MX", {
            style: "currency",
            currency: "MXN"
        });
    }

    abrirCarrito.addEventListener("click", function () {
        seccionCarrito.hidden = false;
        seccionCarrito.scrollIntoView({ behavior: "smooth" });
    });

    cerrarCarrito.addEventListener("click", function () {
        seccionCarrito.hidden = true;
        document.getElementById("productos")
            .scrollIntoView({ behavior: "smooth" });
    });

    vaciarCarrito.addEventListener("click", function () {
        carrito.length = 0;
        mostrarCarrito();
    });

    mostrarCarrito();
});
