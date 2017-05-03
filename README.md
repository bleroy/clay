# Clay

Clay is a dynamic C# type that will enable you to sculpt objects of any shape just as easily as in JavaScript or other dynamic languages.

## Usage

First, you need to instantiate a Clay factory:

```csharp
dynamic New = new ClayFactory();
```

Then you can create objects using a number of different syntaxes:

```csharp
var person = New.Person();
person.FirstName = "Louis";
person.LastName = "Dejardin";
```

```csharp
var person = New.Person();
person["FirstName"] = "Louis";
person["LastName"] = "Dejardin";
```

```csharp
var person = New.Person()
    .FirstName("Louis")
    .LastName("Dejardin");
```

```csharp
var person = New.Person(new {
    FirstName = "Louis",
    LastName = "Dejardin"
});
```

```csharp
var person = New.Person(
    FirstName: "Louis",
    LastName: "Dejardin"
);
```

It is then possible to access members in three different, and equivalent ways:

```csharp
person.FirstName
person["FirstName"]
person.FirstName()
```

One of the most powerful features of Clay is its dynamic casting ability, which enables you to cast a dynamic object to a known interface, without ever having to specify the interface on the object itself:

```csharp
public interface IPerson {
    string FirstName { get; set; }
    string LastName { get; set; }
}

//...

IPerson typedPerson = person;
// Look! IntelliSense! Compile-time checks!
var fullName = $"{typedPerson.FirstName} {typedPerson.LastName}";
```

## Blog posts about Clay

* [Clay: malleable C# dynamic objects – part 1: why we need it](https://weblogs.asp.net/bleroy/clay-malleable-c-dynamic-objects-part-1-why-we-need-it)
* [Clay: malleable C# dynamic objects – part 2](https://weblogs.asp.net/bleroy/clay-malleable-c-dynamic-objects-part-2)
