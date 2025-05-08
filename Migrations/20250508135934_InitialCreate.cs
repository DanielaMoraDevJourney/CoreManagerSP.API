using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreManagerSP.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Administradores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContrasenaHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administradores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntidadesFinancieras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TasaInteres = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IngresoMinimo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RelacionCuotaIngresoMaxima = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AntiguedadHistorialMinima = table.Column<int>(type: "int", nullable: false),
                    AceptaMora = table.Column<bool>(type: "bit", nullable: false),
                    RequiereTarjetaCredito = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntidadesFinancieras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposPrestamo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TipoGeneral = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TasaBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CuotaMinima = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtrosRequisitos = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposPrestamo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contrasena = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ingreso = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NivelHistorialCrediticio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeudasVigentes = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CuotasMensualesComprometidas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NumeroCreditosActivos = table.Column<int>(type: "int", nullable: false),
                    HaTenidoMora = table.Column<bool>(type: "bit", nullable: false),
                    TiempoUltimoIncumplimiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarjetaCredito = table.Column<bool>(type: "bit", nullable: false),
                    AniosHistorialCrediticio = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenAdmins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenAdmins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenAdmins_Administradores_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Administradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntidadesTipoPrestamo",
                columns: table => new
                {
                    EntidadFinancieraId = table.Column<int>(type: "int", nullable: false),
                    TipoPrestamoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntidadesTipoPrestamo", x => new { x.EntidadFinancieraId, x.TipoPrestamoId });
                    table.ForeignKey(
                        name: "FK_EntidadesTipoPrestamo_EntidadesFinancieras_EntidadFinancieraId",
                        column: x => x.EntidadFinancieraId,
                        principalTable: "EntidadesFinancieras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntidadesTipoPrestamo_TiposPrestamo_TipoPrestamoId",
                        column: x => x.TipoPrestamoId,
                        principalTable: "TiposPrestamo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogsSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsSistema", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogsSistema_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Solicitudes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TipoPrestamoId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Plazo = table.Column<int>(type: "int", nullable: false),
                    Proposito = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CuotaEstimadaCliente = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitudes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitudes_TiposPrestamo_TipoPrestamoId",
                        column: x => x.TipoPrestamoId,
                        principalTable: "TiposPrestamo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TokenUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalisisResultados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudPrestamoId = table.Column<int>(type: "int", nullable: false),
                    EntidadFinancieraId = table.Column<int>(type: "int", nullable: false),
                    ProbabilidadAprobacion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CuotaMensualEstimada = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EsApto = table.Column<bool>(type: "bit", nullable: false),
                    MensajeResumen = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalisisResultados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalisisResultados_EntidadesFinancieras_EntidadFinancieraId",
                        column: x => x.EntidadFinancieraId,
                        principalTable: "EntidadesFinancieras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalisisResultados_Solicitudes_SolicitudPrestamoId",
                        column: x => x.SolicitudPrestamoId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistorialMejoras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudPrestamoId = table.Column<int>(type: "int", nullable: false),
                    VariableModificada = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorNuevo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaAplicacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialMejoras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialMejoras_Solicitudes_SolicitudPrestamoId",
                        column: x => x.SolicitudPrestamoId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MejorasSugeridas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalisisResultadoId = table.Column<int>(type: "int", nullable: false),
                    Variable = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ValorSugerido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ImpactoEstimado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsObligatoria = table.Column<bool>(type: "bit", nullable: false),
                    Prioridad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MejorasSugeridas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MejorasSugeridas_AnalisisResultados_AnalisisResultadoId",
                        column: x => x.AnalisisResultadoId,
                        principalTable: "AnalisisResultados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalisisResultados_EntidadFinancieraId",
                table: "AnalisisResultados",
                column: "EntidadFinancieraId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalisisResultados_SolicitudPrestamoId",
                table: "AnalisisResultados",
                column: "SolicitudPrestamoId");

            migrationBuilder.CreateIndex(
                name: "IX_EntidadesTipoPrestamo_TipoPrestamoId",
                table: "EntidadesTipoPrestamo",
                column: "TipoPrestamoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialMejoras_SolicitudPrestamoId",
                table: "HistorialMejoras",
                column: "SolicitudPrestamoId");

            migrationBuilder.CreateIndex(
                name: "IX_LogsSistema_UsuarioId",
                table: "LogsSistema",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MejorasSugeridas_AnalisisResultadoId",
                table: "MejorasSugeridas",
                column: "AnalisisResultadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TipoPrestamoId",
                table: "Solicitudes",
                column: "TipoPrestamoId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_UsuarioId",
                table: "Solicitudes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenAdmins_AdminId",
                table: "TokenAdmins",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenUsuarios_UsuarioId",
                table: "TokenUsuarios",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntidadesTipoPrestamo");

            migrationBuilder.DropTable(
                name: "HistorialMejoras");

            migrationBuilder.DropTable(
                name: "LogsSistema");

            migrationBuilder.DropTable(
                name: "MejorasSugeridas");

            migrationBuilder.DropTable(
                name: "TokenAdmins");

            migrationBuilder.DropTable(
                name: "TokenUsuarios");

            migrationBuilder.DropTable(
                name: "AnalisisResultados");

            migrationBuilder.DropTable(
                name: "Administradores");

            migrationBuilder.DropTable(
                name: "EntidadesFinancieras");

            migrationBuilder.DropTable(
                name: "Solicitudes");

            migrationBuilder.DropTable(
                name: "TiposPrestamo");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
