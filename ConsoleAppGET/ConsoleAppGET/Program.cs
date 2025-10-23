using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PayPal;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudentClient
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
    

        static async Task Main(string[] args)
        {
            int opcion;
            try
            {
                do
                {
                    Console.WriteLine("1-Crear un estudiante");
                    Console.WriteLine("2-Leer un estudiante");
                    Console.WriteLine("3-Actualizar un estudiante");
                    Console.WriteLine("4-Eliminar un estudiante");
                    Console.WriteLine("5-Listado de estudiantes");
                    Console.WriteLine("6-Nombre de estudiantes");
                    Console.WriteLine("7-Ver estadísticas de estudiantes");
                    Console.WriteLine("0-Salir");
                    Console.WriteLine("Ingrese Opcion:");
                    opcion = Convert.ToInt32(Console.ReadLine());
                    switch (opcion)
                    {
                        case 1:
                            Console.WriteLine("Ingresar Nombre:");
                            string? nombre = Console.ReadLine();
                            Console.WriteLine("Ingresar Edad:");
                            int edad = Convert.ToInt32(Console.ReadLine());
                            Student student = new Student
                            {
                                Name = nombre,
                                Age = edad
                            };
                            await CreateStudent(student);
                            break;
                        case 2:
                            Console.WriteLine("Ingresar Codigo:");
                            int codigo = Convert.ToInt32(Console.ReadLine());
                            await ReadStudent(codigo);
                            break;
                        case 3:
                            Console.WriteLine("Ingresar Codigo:");
                            codigo = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Ingresar Nombre:");
                            nombre = Console.ReadLine();
                            Console.WriteLine("Ingresar Edad:");
                            edad = Convert.ToInt32(Console.ReadLine());
                            Student updatedStudent = new Student();
                            updatedStudent.Id = codigo;
                            updatedStudent.Name = nombre;
                            updatedStudent.Age = edad;
                            await UpdateStudent(codigo, updatedStudent);
                            break;
                        case 4:
                            Console.WriteLine("Ingresar Codigo:");
                            codigo = Convert.ToInt32(Console.ReadLine());
                            await DeleteStudent(codigo);
                            break;
                        case 5:
                            await ReadAllStudents();
                            break;
                        case 6:
                            Console.WriteLine("Ingresar Codigo:");
                            codigo = Convert.ToInt32(Console.ReadLine());
                            await ReadName(codigo);
                            break;
                        case 7:
                            var response = await client.GetAsync("https://localhost:7013/Students/ReadAllStudents");

                            if (response.IsSuccessStatusCode)
                            {
                                var estudiantesStats = await response.Content.ReadFromJsonAsync<Student[]>();

                                if (estudiantesStats != null && estudiantesStats.Length > 0)
                                {
                                    var totalEstudiantes = estudiantesStats.Length;
                                    var promedioEdad = estudiantesStats.Average(est => est.Age);
                                    var menoresDe18 = estudiantesStats.Count(est => est.Age < 18);
                                    var entre18y25 = estudiantesStats.Count(est => est.Age >= 18 && est.Age <= 25);
                                    var mayoresDe25 = estudiantesStats.Count(est => est.Age > 25);

                                    Console.WriteLine($"Total estudiantes: {totalEstudiantes}");
                                    Console.WriteLine($"Edad promedio: {promedioEdad:F2}");
                                    Console.WriteLine($"Menores de 18: {menoresDe18}");
                                    Console.WriteLine($"Entre 18 y 25: {entre18y25}");
                                    Console.WriteLine($"Mayores de 25: {mayoresDe25}");
                                }
                                else
                                {
                                    Console.WriteLine("No hay estudiantes registrados.");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Error al obtener estudiantes: {response.StatusCode}");
                            }
                            break;
                        default:
                            Console.WriteLine("Item no contemplado");
                            break;
                    }
                } while (opcion != 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task HelloWorld()
        {
            try
            {
                // Cambia la URL a la de tu API
                string url = "https://localhost:7013/Students/HelloWorld"; // Asegúrate de que el puerto sea correcto

                // Realizar la solicitud GET
                HttpResponseMessage response = await client.GetAsync(url);

                // Asegurarse de que la respuesta sea exitosa
                response.EnsureSuccessStatusCode();

                // Leer el contenido como string
                string responseBody = await response.Content.ReadAsStringAsync();

                // Mostrar el resultado en la consola
                Console.WriteLine("Respuesta del servicio:");
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error al llamar al servicio: {e.Message}");
            }
        }

        private static async Task CreateStudent(Student student)
        {
            // Serializa el objeto Student a JSON
            string jsonStudent = System.Text.Json.JsonSerializer.Serialize(student);
            var content = new StringContent(jsonStudent, Encoding.UTF8, "application/json");

            // Realiza la solicitud POST
            HttpResponseMessage response = await client.PostAsync("https://localhost:7013/Students/CreateStudent", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Estudiante creado exitosamente.");
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al crear el estudiante. Código de estado: {response.StatusCode}, Detalles: {errorContent}");
            }
        }


        private static async Task ReadStudent(int id)
        {
            string url = "https://localhost:7013/Students/ReadStudent/" + id;
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var student = await response.Content.ReadFromJsonAsync<Student>();
                Console.WriteLine($"Student: {student.Name}, Age: {student.Age}");
            }
        }

        private static async Task ReadAllStudents()
        {
            string url = "https://localhost:7013/Students/ReadAllStudents";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var students = await response.Content.ReadFromJsonAsync<Student[]>();
                Console.WriteLine("All Students:");
                foreach (var student in students)
                {
                    Console.WriteLine($"- {student.Name}, Age: {student.Age}");
                }
            }
        }

        private static async Task<bool> UpdateStudent(int id, Student updatedStudent)
        {
            string jsonStudent = JsonConvert.SerializeObject(updatedStudent);
            var content = new StringContent(jsonStudent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync($"https://localhost:7013/Students/UpdateStudent/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Estudiante actualizado exitosamente.");
                return true;
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al actualizar el estudiante: {errorContent}");
                return false;
            }
        }

        private static IActionResult Ok(string v)
        {
            throw new NotImplementedException();
        }

        private static IActionResult StatusCode(int v1, string v2)
        {
            throw new NotImplementedException();
        }

        private static async Task DeleteStudent(int id)
        {
            string url = "https://localhost:7013/Students/DeleteStudent/" + id;
            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Student deleted successfully.");
            }
        }
        private static async Task ReadName(int id)
        {
            string url = "https://localhost:7013/Students/ReadName/" + id;
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var student = await response.Content.ReadFromJsonAsync<ReadStudent>();
                Console.WriteLine($"Student: {student.Name}");
            }
        }

    }

    public class Student
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class ReadStudent
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
    }
}
