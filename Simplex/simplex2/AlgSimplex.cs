using System;

public class AlgSimplex
{
    // Matriz que guardará la tabla del simplex
    private double[,] tabla;

    // Parámetros de entrada: las restricciones, la función objetivo y el tipo de problema
    private double[,] restricciones; // Coeficientes de las restricciones
    private double[] funcionObjetivo; // Coeficientes de la función objetivo
    private string tipoProblema; // Tipo de problema ("Max" o "Min")

    // Parámetros de salida: la solución óptima y el valor óptimo
    private double[] solucion; // Valores óptimos de las variables
    private double valorOptimo; // Valor óptimo de la función objetivo
    private int numVariables; // Número de variables en el problema
    private int numRestricciones; // Número de restricciones en el problema

    // Constructor de la clase Simplex.
    // Inicializa las restricciones y la función objetivo, y convierte el problema de minimización a maximización si es necesario.
    public AlgSimplex(double[,] restricciones, double[] funcionObjetivo, string tipoProblema)
    {
        this.restricciones = restricciones;
        this.funcionObjetivo = funcionObjetivo;
        this.tipoProblema = tipoProblema;


        ConvertirMinimizarAMaximizar();
    }


    // Cambia los signos de los coeficientes de la función objetivo si el problema es de minimización.
    private void ConvertirMinimizarAMaximizar()
    {
        if (tipoProblema.Equals("Min", StringComparison.OrdinalIgnoreCase))
        {
            for (int i = 0; i < funcionObjetivo.Length; i++)
            {
                funcionObjetivo[i] = -funcionObjetivo[i];
            }

        }
    }

    // Inicializa la tabla del simplex con los coeficientes de las restricciones y la función objetivo.
    // Construye la matriz que se utilizará en el algoritmo del simplex.
    public void InicializarTabla()
    {
        numRestricciones = restricciones.GetLength(0);
        numVariables = funcionObjetivo.Length;

        // Crear la tabla con las dimensiones adecuadas
        tabla = new double[numRestricciones + 1, numVariables + numRestricciones + 1];

        // Inicializar todos los elementos en 0
        for (int i = 0; i < tabla.GetLength(0); i++)
        {
            for (int j = 0; j < tabla.GetLength(1); j++)
            {
                tabla[i, j] = 0.0;
            }
        }

        // Llenar la parte de coeficientes de restricciones y RHS
        for (int i = 0; i < numRestricciones; i++)
        {
            for (int j = 0; j < numVariables; j++)
            {
                tabla[i, j] = restricciones[i, j];
            }
            tabla[i, numVariables + i] = 1.0; // Variables de holgura
            tabla[i, numVariables + numRestricciones] = restricciones[i, numVariables]; // RHS
        }

        // Llenar la fila de la función objetivo
        for (int j = 0; j < numVariables; j++)
        {
            tabla[numRestricciones, j] = -funcionObjetivo[j];
        }

        Console.WriteLine("Tabla inicial:");
        ImprimirTabla();
    }

    // Imprime la tabla del simplex en la consola, formateada con dos decimales.
    public void ImprimirTabla()
    {
        // Imprimir la tabla
        for (int i = 0; i < tabla.GetLength(0); i++)
        {
            for (int j = 0; j < tabla.GetLength(1); j++)
            {
                Console.Write($"{tabla[i, j]:F2} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    // Resuelve el problema utilizando el método del simplex.
    // Itera hasta que se cumpla la condición de paro: no hay negativos en la fila de la función objetivo.
    public string Resolver()
    {
        InicializarTabla();
        while (HayNegativos())
        {
            // Obtener la columna pivote
            int columnaPivote = ObtenerColumnaPivote();
            // Obtener la fila pivote
            int filaPivote = ObtenerFilaPivote(columnaPivote);

            // Hacer 1 al pivote
            HacerUno(columnaPivote, filaPivote);
            // Hacer 0 a los demás valores de la columna pivote
            HacerCeros(filaPivote, columnaPivote);

            // ImprimirTabla(); // Descomentar para ver cada iteración
        }

        // Obtener la solución
        return ObtenerSolucion();
    }

    // Verifica si hay negativos en la fila de la función objetivo.
    public bool HayNegativos()
    {
        for (int i = 0; i < tabla.GetLength(1); i++)
        {
            if (tabla[tabla.GetLength(0) - 1, i] < 0)
            {
                return true;
            }
        }
        return false;
    }

    // Obtiene el índice de la columna pivote a partir de la fila de la función objetivo.
    public int ObtenerColumnaPivote()
    {
        for (int i = 0; i < tabla.GetLength(1); i++)
        {
            if (tabla[tabla.GetLength(0) - 1, i] < 0)
            {
                return i;
            }
        }
        return -1;
    }

    // Obtiene el índice de la fila pivote a partir de la columna pivote seleccionada.
    public int ObtenerFilaPivote(int columnaPivote)
    {
        double[] valores = new double[tabla.GetLength(0) - 1];
        for (int i = 0; i < tabla.GetLength(0) - 1; i++)
        {
            valores[i] = tabla[i, tabla.GetLength(1) - 1] / tabla[i, columnaPivote];
        }

        double menor = double.PositiveInfinity;
        int filaPivote = -1;

        for (int i = 0; i < valores.Length; i++)
        {
            if (valores[i] < menor && valores[i] > 0)
            {
                menor = valores[i];
                filaPivote = i;
            }
        }
        return filaPivote;
    }

    // Hace 1 al pivote dividiendo toda la fila pivote entre el valor del pivote.
    public void HacerUno(int columnaPivote, int filaPivote)
    {
        double pivote = tabla[filaPivote, columnaPivote];
        for (int i = 0; i < tabla.GetLength(1); i++)
        {
            tabla[filaPivote, i] /= pivote;
        }
    }

    // Hace 0 a los demás valores de la columna pivote.
    private void HacerCeros(int filaPivote, int columnaPivote)
    {
        for (int i = 0; i < tabla.GetLength(0); i++)
        {
            if (i != filaPivote)
            {
                double valor = tabla[i, columnaPivote];
                for (int j = 0; j < tabla.GetLength(1); j++)
                {
                    tabla[i, j] -= valor * tabla[filaPivote, j];
                }
            }
        }
    }

    // Calcula y muestra la solución óptima.
    public string ObtenerSolucion()
    {
        // Crear un arreglo para almacenar los valores de las variables
        double[] valoresVariables = new double[numVariables];

        // Calcular los valores de las variables a partir de la tabla
        for (int j = 0; j < numVariables; j++)
        {
            bool esVariableBasica = false;
            int filaVariableBasica = -1;

            // Buscar una columna con un 1 en una fila
            for (int i = 0; i < numRestricciones; i++)
            {
                if (tabla[i, j] == 1.0)
                {
                    if (filaVariableBasica == -1)
                    {
                        filaVariableBasica = i;
                    }
                    else
                    {
                        esVariableBasica = false;
                        break;
                    }
                }
                else
                {
                    esVariableBasica = true;
                }
            }

            // Si la variable es básica, su valor es el valor en el lado derecho de la fila correspondiente
            if (esVariableBasica && filaVariableBasica != -1)
            {
                valoresVariables[j] = tabla[filaVariableBasica, tabla.GetLength(1) - 1];
            }
            else
            {
                valoresVariables[j] = 0.0; // La variable no es básica, su valor es 0
            }
        }

        // El valor óptimo de la función objetivo es el valor en la última fila y última columna de la tabla
        valorOptimo = tabla[numRestricciones, tabla.GetLength(1) - 1];

        // Imprimir la solución
        Console.WriteLine($"Solución óptima para {tipoProblema}:");
        Console.WriteLine("Valores de las variables: " + string.Join(", ", valoresVariables));
        Console.WriteLine($"Valor óptimo de Z: {valorOptimo}");

        return $"Solución óptima para {tipoProblema}: " + '\n' + "Valores de las variables: " + string.Join(", ", valoresVariables) + " \n" + $"Valor óptimo de Z: {valorOptimo}";
    }
}
