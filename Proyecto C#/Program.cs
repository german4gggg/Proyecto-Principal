using System;
using System.Collections.Generic;
using System.Linq;

namespace ProyectoBase
{
    // ==========================================
    // 1. VARIABLES GLOBAL / CONSTANTES
    // ==========================================
    public static class ConfiguracionesGlobales
    {
        // const: Constante en tiempo de compilación
        public const string NOMBRE_SISTEMA = "Sistema de Gestión de Empleados";

        // static readonly: Asignada en tiempo de ejecución
        public static readonly DateTime FECHA_INICIALIZACION = DateTime.Now;

        // static: Variable global modificable
        public static int ContadorOperaciones = 0;
    }

    // ==========================================
    // 2. PRINCIPIO SOLID (I & D): INTERFAZ REPOSITORIO
    // ==========================================
    public interface IRepositorio<T>
    {
        void Agregar(T entidad);
        T ObtenerPorId(int id);
        List<T> ObtenerTodos();
    }

    // ==========================================
    // 3. HERENCIA: CLASE ABSTRACTA Y CAMPO PROTECTED
    // ==========================================
    public abstract class EmpleadoBase
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        // Campo con modificador de acceso protected
        protected decimal SalarioBase;

        public EmpleadoBase(int id, string nombre, decimal salarioBase)
        {
            Id = id;
            Nombre = nombre;
            SalarioBase = salarioBase;
        }

        // Método abstracto (POO)
        public abstract decimal CalcularSueldoTotal();

        public override string ToString()
        {
            return $"ID: {Id} | Nombre: {Nombre} | Sueldo Final: S/ {CalcularSueldoTotal():N2}";
        }
    }

    // ==========================================
    // 4. CLASE DERIVADA (HERENCIA Y PRINCIPIO LISKOV)
    // ==========================================
    public class EmpleadoTiempoCompleto : EmpleadoBase
    {
        public decimal Bonificacion { get; set; }

        public EmpleadoTiempoCompleto(int id, string nombre, decimal salarioBase, decimal bonificacion)
            : base(id, nombre, salarioBase)
        {
            Bonificacion = bonificacion;
        }

        // Implementación del método abstracto usando el campo protected SalarioBase
        public override decimal CalcularSueldoTotal()
        {
            return SalarioBase + Bonificacion;
        }
    }

    // ==========================================
    // 5. PATRÓN REPOSITORIO, LINQ, LISTAS Y DICCIONARIOS
    // ==========================================
    public class EmpleadoRepository : IRepositorio<EmpleadoBase>
    {
        // Uso de LISTA
        private readonly List<EmpleadoBase> _empleados = new List<EmpleadoBase>();

        // Uso de DICCIONARIO para auditoría/bitácora
        private readonly Dictionary<int, string> _historialMovimientos = new Dictionary<int, string>();

        public void Agregar(EmpleadoBase empleado)
        {
            // Captura y manejo de errores mediante excepciones explícitas (throw)
            if (empleado == null)
            {
                throw new ArgumentNullException(nameof(empleado), "El empleado no puede ser nulo.");
            }

            // Uso de LINQ (Any) para verificar duplicados
            if (_empleados.Any(e => e.Id == empleado.Id))
            {
                throw new InvalidOperationException($"Ya existe un empleado registrado con el ID {empleado.Id}.");
            }

            _empleados.Add(empleado);
            _historialMovimientos[empleado.Id] = $"Registrado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            ConfiguracionesGlobales.ContadorOperaciones++;
        }

        public EmpleadoBase ObtenerPorId(int id)
        {
            // Uso de LINQ (FirstOrDefault)
            var empleado = _empleados.FirstOrDefault(e => e.Id == id);

            if (empleado == null)
            {
                throw new KeyNotFoundException($"No se encontró ningún empleado con el ID {id}.");
            }

            return empleado;
        }

        public List<EmpleadoBase> ObtenerTodos()
        {
            return _empleados;
        }

        // Consulta LINQ para filtrar y ordenar
        public List<EmpleadoBase> ObtenerEmpleadosConSueldoMayorA(decimal montoMinimo)
        {
            return _empleados
                .Where(e => e.CalcularSueldoTotal() > montoMinimo)
                .OrderBy(e => e.Nombre)
                .ToList();
        }

        public string ObtenerHistorial(int id)
        {
            if (_historialMovimientos.TryGetValue(id, out string detalle))
            {
                return detalle;
            }
            return "Sin historial disponible.";
        }
    }

    // ==========================================
    // 6. CLASE ENCARGADA DEL MENÚ Y SELECCIÓN DE OPCIONES
    // ==========================================
    public class MenuPrincipal
    {
        private readonly EmpleadoRepository _repositorio = new EmpleadoRepository();

        public void Iniciar()
        {
            bool continuar = true;

            while (continuar)
            {
                Console.Clear();
                Console.WriteLine("==================================================");
                Console.WriteLine($"   {ConfiguracionesGlobales.NOMBRE_SISTEMA}");
                Console.WriteLine($"   SISTEMA INICIADO: {ConfiguracionesGlobales.FECHA_INICIALIZACION:dd/MM/yyyy HH:mm}");
                Console.WriteLine($"   OPERACIONES REALIZADAS: {ConfiguracionesGlobales.ContadorOperaciones}");
                Console.WriteLine("==================================================");
                Console.WriteLine("1. Registrar nuevo empleado");
                Console.WriteLine("2. Buscar empleado por ID");
                Console.WriteLine("3. Mostrar todos los empleados");
                Console.WriteLine("4. Filtrar empleados con sueldo mayor a un monto (LINQ)");
                Console.WriteLine("5. Salir");
                Console.WriteLine("--------------------------------------------------");
                Console.Write("Seleccione una opción: ");

                string opcion = Console.ReadLine();

                // Manejo de errores con TRY - CATCH
                try
                {
                    switch (opcion)
                    {
                        case "1":
                            RegistrarEmpleado();
                            break;
                        case "2":
                            BuscarEmpleado();
                            break;
                        case "3":
                            MostrarTodos();
                            break;
                        case "4":
                            FiltrarPorSueldo();
                            break;
                        case "5":
                            continuar = false;
                            Console.WriteLine("\nSaliendo del sistema...");
                            break;
                        default:
                            Console.WriteLine("\n[Advertencia] Opción no válida. Presione cualquier tecla para reintentar.");
                            Console.ReadKey();
                            break;
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"\n[Error de Formato]: Ha ingresado un dato con formato incorrecto. Detalle: {ex.Message}");
                    Pausar();
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"\n[Error de Negocio]: {ex.Message}");
                    Pausar();
                }
                catch (KeyNotFoundException ex)
                {
                    Console.WriteLine($"\n[Error de Búsqueda]: {ex.Message}");
                    Pausar();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[Error Inesperado]: {ex.Message}");
                    Pausar();
                }
            }
        }

        private void RegistrarEmpleado()
        {
            Console.WriteLine("\n--- REGISTRO DE EMPLEADO ---");

            Console.Write("Ingrese ID (Numérico): ");
            int id = int.Parse(Console.ReadLine()); // Puede lanzar FormatException

            Console.Write("Ingrese Nombre completo: ");
            string nombre = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new Exception("El nombre no puede estar vacío."); // Lanzando Exception con throw
            }

            Console.Write("Ingrese Salario Base: ");
            decimal salario = decimal.Parse(Console.ReadLine());

            Console.Write("Ingrese Bonificación: ");
            decimal bonificacion = decimal.Parse(Console.ReadLine());

            // Crear objeto aplicando herencia y POO
            EmpleadoBase nuevoEmpleado = new EmpleadoTiempoCompleto(id, nombre, salario, bonificacion);

            _repositorio.Agregar(nuevoEmpleado);

            Console.WriteLine("\n¡Empleado registrado exitosamente!");
            Pausar();
        }

        private void BuscarEmpleado()
        {
            Console.WriteLine("\n--- BÚSQUEDA DE EMPLEADO ---");
            Console.Write("Ingrese ID a buscar: ");
            int id = int.Parse(Console.ReadLine());

            EmpleadoBase emp = _repositorio.ObtenerPorId(id);

            Console.WriteLine("\n[Empleado Encontrado]");
            Console.WriteLine(emp.ToString());
            Console.WriteLine($"Historial: {_repositorio.ObtenerHistorial(emp.Id)}");
            Pausar();
        }

        private void MostrarTodos()
        {
            Console.WriteLine("\n--- LISTA GENERAL DE EMPLEADOS ---");
            var lista = _repositorio.ObtenerTodos();

            if (!lista.Any())
            {
                Console.WriteLine("No hay empleados registrados.");
            }
            else
            {
                foreach (var emp in lista)
                {
                    Console.WriteLine(emp.ToString());
                }
            }
            Pausar();
        }

        private void FiltrarPorSueldo()
        {
            Console.WriteLine("\n--- FILTRAR EMPLEADOS (LINQ) ---");
            Console.Write("Ingrese el monto mínimo de sueldo: ");
            decimal monto = decimal.Parse(Console.ReadLine());

            var resultado = _repositorio.ObtenerEmpleadosConSueldoMayorA(monto);

            Console.WriteLine($"\nResultados con sueldo mayor a S/ {monto:N2}:");
            if (!resultado.Any())
            {
                Console.WriteLine("No se encontraron registros que cumplan la condición.");
            }
            else
            {
                foreach (var emp in resultado)
                {
                    Console.WriteLine(emp.ToString());
                }
            }
            Pausar();
        }

        private void Pausar()
        {
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }

    // ==========================================
    // 7. PROGRAM.CS (SOLO CONTIENE LA EJECUCIÓN MAIN)
    // ==========================================
    internal class Program
    {
        static void Main(string[] args)
        {
            // Única función de Main: Instanciar e iniciar el menú
            MenuPrincipal app = new MenuPrincipal();
            app.Iniciar();
        }
    }
}