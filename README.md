# Hanabi-Lang  
A Programming Language that use C# as interpreter  

## Basic Types  
### Primitive  
* int  
* bool  
* str  
* float  
* decimal  
### Non-Primitive Types
* List  
* Dict  

## Define variables  
### Mutable  
```
// var varableName = value;  
// Type not provided:  

var text = "Hello World";  
var value = 12345;  
var values = [1, 2, 3, 4, 5];  
var keyValues = { "a": 1, "b": 2 };  

// Type provided:  

var text: str = "Hello World";  
var value: int = 12345;  
var values: List = [1, 2, 3, 4, 5];  
var keyValues: Dict = { "a": 1, "b": 2 };  
```

### Immutable   
```
// Note that elements in List and Dict are still mutable, due to it is not reassigning the variable.  
// var varableName = value; 
// Type not provided:  

const text = "Hello World";  
const value = 12345;  
const pi = 3.14;  
const values = [1, 2, 3, 4, 5];  
const keyValues = { "a": 1, "b": 2 };  

// Type provided:  

const text: str = "Hello World";  
const value: int = 12345;  
const pi: float = 3.14;  
const values: List = [1, 2, 3, 4, 5];  
const keyValues: Dict = { "a": 1, "b": 2 };  
```

## Define functions  
Every function default return null value.  

### Normal function  
fn functionName() {  
    println("Hello World");  
}   

```
fn Add(left, right) {  
    return left + right;  
}  

fn Add(left: int, right: int) {  
    return left + right;  
}  
```

### Normal one line function  

```
// In one line function, it default return a value of the following expression.  
// fn functionName() => println("Hello World");  
fn Add(left, right) => left + right;  

fn Add(left: int, right: int) => left + right;  
```

### Lambda function  
const functionName = () => {  
    println("Hello World");  
}   

```
const Add = (left, right) => {  
    return left + right;  
}  

const Add = (left: int, right: int) => {  
    return left + right;  
}  
```

### Lambda one line function    

```
// In one line function, it default return a value of the following expression.  
// fn functionName() => println("Hello World");

const Add = (left, right) => left + right;  

const Add = (left: int, right: int) => left + right;  
```
