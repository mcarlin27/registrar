using System.Collections.Generic;
using System.Data.SqlClient;
using System;
using System.IO;

namespace Registrar
{
  public class Student
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public int Year { get; set; }
    public DateTime Enrollment { get; set; }

    public Student(string name, int year, DateTime enrollment, int id = 0)
    {
      Id = id;
      Name = name;
      Year = year;
      Enrollment = enrollment;
    }

    public override bool Equals(System.Object otherStudent)
    {
      if (!(otherStudent is Student))
      {
        return false;
      }
      else
      {
        Student newStudent = (Student) otherStudent;
        bool idEquality = this.Id == newStudent.Id;
        bool nameEquality = this.Name == newStudent.Name;
        bool yearEquality = this.Year == newStudent.Year;
        bool enrollmentEquality = this.Enrollment == newStudent.Enrollment;

        return (idEquality && nameEquality && yearEquality && enrollmentEquality);
      }
    }

    public void Save()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("INSERT INTO students (name, year, enrollment) OUTPUT INSERTED.id VALUES (@StudentName, @StudentYear, @StudentEnrollment);", conn);

      SqlParameter nameParameter = new SqlParameter();
      nameParameter.ParameterName = "@StudentName";
      nameParameter.Value = this.Name;

      SqlParameter studentYearParameter = new SqlParameter();
      studentYearParameter.ParameterName = "@StudentYear";
      studentYearParameter.Value = this.Year;

      SqlParameter studentEnrollmentParameter = new SqlParameter();
      studentEnrollmentParameter.ParameterName = "@StudentEnrollment";
      studentEnrollmentParameter.Value = this.Enrollment;

      cmd.Parameters.Add(nameParameter);
      cmd.Parameters.Add(studentYearParameter);
      cmd.Parameters.Add(studentEnrollmentParameter);

      SqlDataReader rdr = cmd.ExecuteReader();

      while(rdr.Read())
      {
        this.Id = rdr.GetInt32(0);
      }
      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }
    }

    public static List<Student> GetAll()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT * FROM students;", conn);
      SqlDataReader rdr = cmd.ExecuteReader();

      List<Student> students = new List<Student>{};
      while (rdr.Read())
      {
        int studentId = rdr.GetInt32(0);
        string studentName = rdr.GetString(1);
        int studentYear = rdr.GetInt32(2);
        DateTime studentEnrollment = Convert.ToDateTime(rdr.GetString(3));
        Student newStudent = new Student(studentName, studentYear, studentEnrollment, studentId);
        students.Add(newStudent);
      }

      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }
      return students;
    }

    public void UpdateStudent(int newYear, string newName)
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("UPDATE students SET year = @NewYear, name = @NewName OUTPUT INSERTED.year, INSERTED.name WHERE id = @StudentId;", conn);

      SqlParameter newYearParameter = new SqlParameter();
      newYearParameter.ParameterName = "@NewYear";
      newYearParameter.Value = newYear;
      cmd.Parameters.Add(newYearParameter);

      SqlParameter newNameParameter = new SqlParameter();
      newNameParameter.ParameterName = "@NewName";
      newNameParameter.Value = newName;
      cmd.Parameters.Add(newNameParameter);

      SqlParameter studentIdParameter = new SqlParameter();
      studentIdParameter.ParameterName = "@StudentId";
      studentIdParameter.Value = this.Id;
      cmd.Parameters.Add(studentIdParameter);
      SqlDataReader rdr = cmd.ExecuteReader();

      while(rdr.Read())
      {
        this.Year = rdr.GetInt32(0);
        this.Name = rdr.GetString(1);
      }
      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }
    }

    public static Student Find(int id)
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT * FROM students WHERE id = @StudentId;", conn);
      SqlParameter studentIdParameter = new SqlParameter();
      studentIdParameter.ParameterName = "@StudentId";
      studentIdParameter.Value = id.ToString();
      cmd.Parameters.Add(studentIdParameter);
      SqlDataReader rdr = cmd.ExecuteReader();

      int foundStudentId = 0;
      string foundStudentName = null;
      int foundStudentYear = 0;
      DateTime foundStudentEnrollment = default(DateTime);
      while(rdr.Read())
      {
        foundStudentId = rdr.GetInt32(0);
        foundStudentName = rdr.GetString(1);
        foundStudentYear = rdr.GetInt32(2);
        foundStudentEnrollment = Convert.ToDateTime(rdr.GetString(3));
      }
      Student foundStudent = new Student(foundStudentName, foundStudentYear, foundStudentEnrollment, foundStudentId);

      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }
      return foundStudent;
    }

    public void AddCourse(Course course)
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("INSERT INTO students_courses (student_id, course_id) VALUES (@StudentId, @CourseId);", conn);

      SqlParameter studentIdParameter = new SqlParameter();
      studentIdParameter.ParameterName = "@StudentId";
      studentIdParameter.Value = this.Id;
      cmd.Parameters.Add(studentIdParameter);

      SqlParameter courseIdParameter = new SqlParameter();
      courseIdParameter.ParameterName = "@CourseId";
      courseIdParameter.Value = course.Id;
      cmd.Parameters.Add(courseIdParameter);

      cmd.ExecuteNonQuery();
      if (conn != null)
      {
        conn.Close();
      }
    }

    public List<Course> GetCourses()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT courses.* FROM students JOIN students_courses ON (students.id = students_courses.student_id) JOIN courses ON (students_courses.course_id = courses.id)  WHERE students.id = @StudentId;", conn);

      SqlParameter studentIdParameter = new SqlParameter();
      studentIdParameter.ParameterName = "@StudentId";
      studentIdParameter.Value = this.Id;

      cmd.Parameters.Add(studentIdParameter);
      SqlDataReader rdr = cmd.ExecuteReader();

      List<Course> courses = new List<Course>{};

      while(rdr.Read())
      {
        int courseId = rdr.GetInt32(0);
        string courseName = rdr.GetString(1);
        int courseTeacherId = rdr.GetInt32(2);

        Course newCourse = new Course(courseName, courseTeacherId, courseId);
        courses.Add(newCourse);
      }

      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }
      return courses;
    }

    public void AddTeacher(Teacher teacher)
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("INSERT INTO students_teachers (student_id, teacher_id) VALUES (@StudentId, @TeacherId);", conn);

      SqlParameter studentIdParameter = new SqlParameter();
      studentIdParameter.ParameterName = "@StudentId";
      studentIdParameter.Value = this.Id;
      cmd.Parameters.Add(studentIdParameter);

      SqlParameter teacherIdParameter = new SqlParameter();
      teacherIdParameter.ParameterName = "@TeacherId";
      teacherIdParameter.Value = teacher.Id;
      cmd.Parameters.Add(teacherIdParameter);

      cmd.ExecuteNonQuery();
      if (conn != null)
      {
        conn.Close();
      }
    }

    public List<Teacher> GetTeachers()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT teachers.* FROM students JOIN students_teachers ON (students.id = students_teachers.student_id) JOIN teachers ON (students_teachers.teacher_id = teachers.id)  WHERE students.id = @StudentId;", conn);

      SqlParameter studentIdParameter = new SqlParameter();
      studentIdParameter.ParameterName = "@StudentId";
      studentIdParameter.Value = this.Id;

      cmd.Parameters.Add(studentIdParameter);
      SqlDataReader rdr = cmd.ExecuteReader();

      List<Teacher> teachers = new List<Teacher>{};

      while(rdr.Read())
      {
        int teacherId = rdr.GetInt32(0);
        string teacherName = rdr.GetString(1);

        Teacher newTeacher = new Teacher(teacherName, teacherId);
        teachers.Add(newTeacher);
      }

      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }
      return teachers;
    }

    public void Delete()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("DELETE FROM students WHERE id = @StudentId;", conn);

      SqlParameter studentIdParameter = new SqlParameter();
      studentIdParameter.ParameterName = "@StudentId";
      studentIdParameter.Value = this.Id;

      cmd.Parameters.Add(studentIdParameter);
      cmd.ExecuteNonQuery();

      if (conn != null)
      {
        conn.Close();
      }
    }

    public static void DeleteAll()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();
      SqlCommand cmd = new SqlCommand("DELETE FROM students;", conn);
      cmd.ExecuteNonQuery();
      conn.Close();
    }
  }
}
