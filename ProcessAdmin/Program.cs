using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProcessAdmin
{
    class Program
    {
        private enum Accion { Eliminar, Sumarizar, Consultar, Volver };

        static void Main(string[] args)
        {
            string Proceso = args.Any() ? args[0] : ObtenerNombreProceso();

            Console.WriteLine("\n");
            Accion AccionSeleccionada = Acciones();

            if (AccionSeleccionada.Equals(Accion.Volver))
            {
                Console.WriteLine("\n");
                Main(args);
            }
            else
            {
                if (!ConfirmarAccion(AccionSeleccionada, Proceso))
                {
                    Console.WriteLine("\n-------\n");
                    Main(new string[] { Proceso });
                }
                else
                {
                    Console.WriteLine("\nEsepere...");
                    switch (AccionSeleccionada)
                    {
                        case Accion.Eliminar: Eliminar(Proceso); break;
                        case Accion.Sumarizar: Sumarizar(Proceso); break;
                        case Accion.Consultar: CrearConsulta(Process.GetProcessesByName(Proceso)); break;
                        case Accion.Volver: break;
                        default: break;
                    }
                }
            }
        }

        private static void Eliminar(string proceso)
        {
            Console.WriteLine("\n");

            char c = '|';
            var chars = new[] { '|', '/', '-', '\\' };

            int Cantidad = 0;
            string Session = Process.GetCurrentProcess().SessionId.ToString();

            Console.WriteLine("Ejecutando Accion... " + c + " \n");

            foreach (Process mi_proceso in Process.GetProcessesByName(proceso))
            {

                int CTop = Console.CursorTop - 2;
                Console.SetCursorPosition(0, CTop);


                int indexOf = Array.IndexOf(chars, c);

                indexOf = indexOf == chars.Length - 1 ? 0 : ++indexOf;

                c = chars[indexOf];

                Console.WriteLine("Ejecutando Accion... " + c + " \n");

                if (mi_proceso.ProcessName.ToLower() == proceso && Session.Equals(mi_proceso.SessionId.ToString()))
                {
                    mi_proceso.WaitForExit(2000);

                    if (!mi_proceso.HasExited)
                    {
                        mi_proceso.Kill();
                        Cantidad++;
                    }
                }
            }

            int CTop_ = Console.CursorTop - 2;
            Console.SetCursorPosition(0, CTop_);

            Console.WriteLine("Ejecutando Accion...                     \n");

            Console.WriteLine("\nSe han finalizazdo {0} procesos.\n-------\n", Cantidad);

            Console.WriteLine("\nPresione Enter para Reiniciar...");
            Console.ReadLine();
            Console.WriteLine("\n-------\n");
        }

        private static void Sumarizar(string proceso)
        {
            Console.WriteLine("\n");

            int Cantidad = 0;
            long Sumarizacion = 0;

            var procesos = Process.GetProcessesByName(proceso);

            Cantidad = procesos.Count();
            Sumarizacion = procesos.Sum(x => x.PrivateMemorySize64);

            Console.WriteLine("El Id. de sesión actual es el Id. {0}", Process.GetCurrentProcess().SessionId.ToString());
            Console.WriteLine("\nExistes {0} procesos, con una suma total de {1} Bytes [{2:0000.0000} MB].\n", Cantidad, Sumarizacion, Sumarizacion / 1024 / 1024);

            List<int> SessionIds = procesos.Select(x => x.SessionId).Distinct().ToList();

            foreach (int S_id in SessionIds)
            {
                var procesosSession = procesos.Where(x => x.SessionId == S_id);

                Cantidad = procesosSession.Count();
                Sumarizacion = procesosSession.Sum(x => x.PrivateMemorySize64);

                Console.WriteLine("\tEl Id. de sesión {0} cuenta con {1} procesos, con una suma total de {2} Bytes [{2:0000.0000} MB].\n", S_id, Cantidad, Sumarizacion, ((Sumarizacion / 1024) / 1024));
            }


            Console.WriteLine("\nPresione Enter para Reiniciar...");
            Console.ReadLine();
            Console.WriteLine("\n-------\n");
        }

        private static bool ConfirmarAccion(Accion accion, string Proceso)
        {
            return accion switch
            {
                Accion.Eliminar => Respuesta("Confirma que desea finalizar todos los procesos activos llamados", Proceso),
                Accion.Sumarizar => Respuesta("Confirma que desea sumarizar el Consumo de Espacio de Trabajo de todos los procesos activos llamados", Proceso),
                Accion.Consultar => Respuesta("Confirma que desea consultar el Consumo de Memoria de todos los procesos activos llamados", Proceso),
                Accion.Volver => false,
                _ => false,
            };
        }

        private static bool Respuesta(string mensaje, string proceso)
        {
            Console.WriteLine($"\n{mensaje} '{proceso}'? S/N");

            return Console.ReadKey().KeyChar switch
            {
                'S' => true,
                's' => true,
                'N' => false,
                'n' => false,
                _ => Respuesta(mensaje, proceso),
            };
        }

        private static string ObtenerNombreProceso()
        {
            Console.WriteLine("Escriba el Nombre del Proceso, al finalizar precione Enter: ");

            string rta = Console.ReadLine().Trim();

            return string.IsNullOrEmpty(rta) ? ObtenerNombreProceso() : rta;
        }

        private static Accion Acciones()
        {
            Console.WriteLine("Seleccione la acción a realizar:\n1. Eliminar\n2. Sumarizar Espacio de Trabajo\n3. Consultar.\nEsc. Volver");

            char accion = Console.ReadKey().KeyChar;

            switch (accion)
            {
                case '1': return Accion.Eliminar;
                case '2': return Accion.Sumarizar;
                case '3': return Accion.Consultar;
                case (char)27: return Accion.Volver;
                default:
                    {
                        Console.WriteLine(" ----> Opción Incorrecta!!\n");
                        return Acciones();
                    }
            }
        }

        private static void CrearConsulta(Process[] Procesos)
        {
            if (!Procesos.Any()) { Console.WriteLine("\nNo existen procesos activos.\n"); return; }

            int Referencia = 1024;
            int Cantidad = 0;

            Console.WriteLine("\nPID\tWorkingSet\tPrivateMemorySize64\tPeakVirtualMemorySize\tPagedSystemMemorySize\tPagedMemorySize\tNonpagedSystemMemorySize\n");

            foreach (Process mi_proceso in Procesos.Where(p => p.SessionId == Process.GetCurrentProcess().SessionId))
            {
                Console.WriteLine("{0}\t\t{1}\t\t{2}\t\t\t{3}\t\t\t{4}\t\t\t{5}\t\t{6}\t", mi_proceso.Id, mi_proceso.WorkingSet64 / Referencia, mi_proceso.PrivateMemorySize64 / Referencia, mi_proceso.PeakVirtualMemorySize64 / Referencia, mi_proceso.PagedSystemMemorySize64 / Referencia, mi_proceso.PagedMemorySize64 / Referencia, mi_proceso.NonpagedSystemMemorySize64 / Referencia);
                Cantidad++;
            }

            Console.WriteLine($"\nSe han consultado {Cantidad} procesos.\n-------\n");

            Console.WriteLine("\nPresione Enter para Reiniciar...");
            Console.ReadLine();
            Console.WriteLine("\n-------\n");
        }
    }
}