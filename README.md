# Hanabi-Lang  
A Programming Language that use C# as interpreter  

File extension: .hnb  

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
// let varableName = value;  
// Type not provided:  

let text = "Hello World";  
let value = 12345;  
let values = [1, 2, 3, 4, 5];  
let keyValues = { "a": 1, "b": 2 };  

// Type provided:  

let text: str = "Hello World";  
let value: int = 12345;  
let values: List = [1, 2, 3, 4, 5];  
let keyValues: Dict = { "a": 1, "b": 2 };  
```

### Immutable   
```
// Note that elements in List and Dict are still mutable, due to it is not reassigning the variable.  
// let varableName = value; 
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

```
// fn functionName() {  
//     println("Hello World");  
// }   

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

```
// const functionName = () => {  
//     println("Hello World");  
// }   

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

### Catch Expression  

```
// catch without default value, last variable must be error.  

let a, b, error = catch(null + null); // a: null, b: null, error: Exception  

let a, error = catch(3.14 * 3.14); // a: 9.8596, error: null  

let a = catch(3.14 * 3.14); // a: null  

// catch with default value  

let a = catch(int("Test"), 0); // a: 0  

let a = catch(int("12345"), 0); // a: 12345  

```
