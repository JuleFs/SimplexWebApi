using Microsoft.AspNetCore.Mvc;
using Simplex.Modelos;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Simplex.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimplexController : ControllerBase
    {
        // Sobrecarga 1: Aceptar datos como string
        [HttpPost("recibir-string")]
        public IActionResult RecibirString([FromBody] ParametrosAlgoritmo datos)
        {
            if (datos == null)
            {
                return BadRequest("El string no puede estar vacío.");
            }

            int filas = datos.Matriz.Length;
            int columnas = datos.Matriz[0].Length;
            double[,] restricciones = new double[filas, columnas];

            // Copiar valores del arreglo de arreglos a la matriz multidimensional
            for (int i = 0; i < filas; i++)
            {
                for (int j = 0; j < columnas; j++)
                {
                    restricciones[i, j] = datos.Matriz[i][j];
                }
            }

            double[] funcionObjetivo = datos.Arreglo;
            string proceso = datos.Texto;

            AlgSimplex simplexMax = new AlgSimplex(restricciones, funcionObjetivo, proceso);

            // Procesa los datos (aquí solo estamos devolviéndolos como respuesta)
            return Ok(new { Mensaje = $"{simplexMax.Resolver()}", Datos = datos });
        }

        // Sobrecarga 2: Aceptar un archivo de texto
        [HttpPost("recibir-archivo")]
        public async Task<IActionResult> RecibirArchivo([FromForm] IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest("El archivo está vacío o no fue enviado.");
            }

            // Procesar el archivo de texto (ejemplo: leer su contenido)
            using (var reader = new StreamReader(archivo.OpenReadStream()))
            {
                string contenido = await reader.ReadToEndAsync();

                // Extraer las restricciones, función objetivo y proceso del archivo
                double[,] restricciones = ExtraerRestricciones(contenido);
                double[] funcionObjetivo = ExtraerFuncionObjetivo(contenido);
                string proceso = ExtraerProceso(contenido);

                AlgSimplex simplexMax = new AlgSimplex(restricciones, funcionObjetivo, proceso);

                // Aquí puedes procesar el contenido del archivo
                return Ok(new { Mensaje = $"{simplexMax.Resolver()}" });
            }
        }

        // Método para extraer restricciones y convertir a matriz multidimensional
        private double[,] ExtraerRestricciones(string contenido)
        {
            // Usamos expresiones regulares para buscar la sección de restricciones
            var regex = new Regex(@"restricciones:\s*\[\[(.*?)\]\]", RegexOptions.Singleline);
            var match = regex.Match(contenido);

            if (match.Success)
            {
                // Extraemos el contenido entre los corchetes y lo dividimos por filas
                string datosRestricciones = match.Groups[1].Value;
                string[] filas = datosRestricciones.Split("],[");

                // Inicializamos la matriz multidimensional
                int filasCount = filas.Length;
                int columnasCount = filas[0].Split(',').Length;
                double[,] matriz = new double[filasCount, columnasCount];

                // Rellenamos la matriz
                for (int i = 0; i < filasCount; i++)
                {
                    string[] valoresFila = filas[i].Split(',');
                    for (int j = 0; j < columnasCount; j++)
                    {
                        matriz[i, j] = double.Parse(valoresFila[j].Trim());
                    }
                }
                return matriz;
            }
            throw new InvalidDataException("No se encontraron restricciones en el archivo.");
        }

        // Método para extraer la función objetivo y convertirla a un arreglo
        private double[] ExtraerFuncionObjetivo(string contenido)
        {
            // Usamos expresiones regulares para buscar la sección de función objetivo
            var regex = new Regex(@"funcionObjetivo:\s*\[(.*?)\]", RegexOptions.Singleline);
            var match = regex.Match(contenido);

            if (match.Success)
            {
                // Extraemos el contenido entre los corchetes y lo convertimos a un arreglo
                string datosFuncionObjetivo = match.Groups[1].Value;
                string[] valores = datosFuncionObjetivo.Split(',');

                // Convertimos los valores a un arreglo de double
                double[] arreglo = new double[valores.Length];
                for (int i = 0; i < valores.Length; i++)
                {
                    arreglo[i] = double.Parse(valores[i].Trim());
                }
                return arreglo;
            }
            throw new InvalidDataException("No se encontró la función objetivo en el archivo.");
        }

        // Método para extraer el proceso (Max o Min) y convertirlo a string
        private string ExtraerProceso(string contenido)
        {
            // Usamos expresiones regulares para buscar la sección del proceso
            var regex = new Regex(@"proceso:\s*(Max|Min)", RegexOptions.Singleline);
            var match = regex.Match(contenido);

            if (match.Success)
            {
                // Retornamos el valor de proceso encontrado
                return match.Groups[1].Value.Trim();
            }
            throw new InvalidDataException("No se encontró el proceso en el archivo.");
        }
    }
}
