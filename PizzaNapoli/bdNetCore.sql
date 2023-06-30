use master
--
Create database PizzaVentaNetCore
use PizzaVentaNetCore

Create table Usuario(
  id_usuario INT PRIMARY KEY IDENTITY,
  nombreusuario VARCHAR(100),
  contrasenausuario VARCHAR(100),
  emailusuario varchar(100)
);
INSERT INTO Usuario VALUES ('viktto', '123','viktto@gmail.com'),('facu', '456','facu@gmail.com'),('jose', '123','jgonzales@gmail.com')
Select * from Usuario

--

CREATE TABLE Pizza (
  id_pizza INT PRIMARY KEY,
  nombrepizza VARCHAR(50),
  precio DECIMAL(8, 2),
  descripcion VARCHAR(100)
);
INSERT INTO Pizza (id_pizza, nombrepizza, precio, descripcion)
VALUES
  (1, 'Pizza Margarita', 10.99, 'Deliciosa pizza con salsa de tomate, queso mozzarella y albahaca.'),
  (2, 'Pizza Hawaiana', 12.99, 'Pizza con jam�n, pi�a, salsa de tomate y queso mozzarella.'),
  (3, 'Pizza Pepperoni', 11.99, 'Pizza con rodajas de pepperoni, salsa de tomate y queso mozzarella.'),
  (4, 'Pizza Barbacoa', 13.99, 'Pizza con salsa barbacoa, pollo desmenuzado, cebolla y queso mozzarella.'),
  (5, 'Pizza Cuatro Quesos', 12.99, 'Pizza con una deliciosa combinaci�n de quesos: mozzarella, cheddar, gorgonzola y parmesano.'),
  (6, 'Pizza Vegetariana', 11.99, 'Pizza con variedad de vegetales frescos, como champi�ones, pimientos y aceitunas.'),
  (7, 'Pizza Mexicana', 13.99, 'Pizza con carne de res sazonada, jalape�os, pimientos y salsa picante.'),
  (8, 'Pizza Napolitana', 12.99, 'Pizza con salsa de tomate, anchoas, aceitunas y alcaparras.'),
  (9, 'Pizza Caprichosa', 11.99, 'Pizza con jam�n, champi�ones, alcachofas y aceitunas negras.'),
  (10, 'Pizza BBQ Pollo', 13.99, 'Pizza con pollo a la parrilla, salsa barbacoa, cebolla y queso mozzarella.'),
  (11, 'Pizza Supreme', 12.99, 'Pizza con pepperoni, salchicha, pimientos, cebolla y aceitunas.'),
  (12, 'Pizza de At�n', 11.99, 'Pizza con at�n en aceite, cebolla, aceitunas y salsa de tomate.'),
  (13, 'Pizza Marinara', 12.99, 'Pizza tradicional con salsa de tomate, ajo, or�gano y aceite de oliva.');

  select * from Pizza


CREATE TABLE Pedido (
  id_pedido INT PRIMARY KEY IDENTITY,
  id_usuario INT,
  fecha_pedido DATETIME DEFAULT (GETDATE()),
  FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);


CREATE TABLE DetallePedido (
  id_pedido INT,
  id_pizza INT,
  cantidad INT,
  precio DECIMAL,
  FOREIGN KEY (id_pedido) REFERENCES Pedido(id_pedido),
  FOREIGN KEY (id_pizza) REFERENCES Pizza(id_pizza)
);


CREATE TABLE Direccion (
  id_direccion INT PRIMARY KEY,
  id_usuario INT,
  calle VARCHAR(100),
  ciudad VARCHAR(50),
  FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
INSERT INTO Direccion (id_direccion, id_usuario, calle, ciudad)
VALUES (1, 1, 'Calle Principal 123', 'Ciudad A');

INSERT INTO Direccion (id_direccion, id_usuario, calle, ciudad)
VALUES (2, 2, 'Avenida Central 456', 'Ciudad B');


ALTER TABLE Usuario
ADD CONSTRAINT UQ_nombreusuario UNIQUE (nombreusuario);
GO


--PROCEDURES
--LISTA DE PIZZA
CREATE OR ALTER PROC usp_pizza
AS
Select *
FROM Pizza
GO
--REGISTRAR PEDIDO 
CREATE OR ALTER PROC usp_add_pedido
@idusu INT
AS
INSERT Pedido(id_usuario) VALUES(@idusu)
GO

CREATE OR ALTER PROC usp_detallepedido
@id INT,
@idpizza INT,
@cantidad INT,
@pre DECIMAL
AS
INSERT DetallePedido VALUES(@id,@idpizza,@cantidad,@pre)
GO


create or alter proc usp_users
As
Select * from Usuario
go

create or alter proc usp_register_users
@nombre varchar(50),
@email varchar(100),
@clave varchar(100)
as
begin
insert into Usuario Values (@nombre,@clave,@email)
end
go


CREATE OR ALTER PROC usp_add_pedido
@idusu INT,
@idpedido INT OUTPUT
AS
BEGIN
    INSERT INTO Pedido (id_usuario) VALUES (@idusu);
    SET @idpedido = SCOPE_IDENTITY();
    SELECT @idpedido;
END
GO


CREATE OR ALTER PROC USP_Historial_pedido
@idusuario int
as
begin
	Select pe.id_pedido,
		   pe.fecha_pedido,
		   p.nombrepizza,
		   d.cantidad,
		   d.precio
	from DetallePedido d
	Join Pizza p on p.id_pizza= d.id_pizza
	Join pedido pe on pe.id_pedido = d.id_pedido
	where pe.id_usuario=@idusuario
	order by id_pedido asc
end
go


