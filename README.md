# TwinCatToCouchDB
A small hack to read TwinCat REAL variables via ADS and send them to CouchDB.

This program reads REAL variables from a Beckhoff PLC and writes them to CouchDB.
All details for making the connection to the PLC and CouchDB database are contained in a
config file called data.conf. It should be placed at the root of the executable.

This software depends on JSON.NET and TwinCat ADS libraries.

Author: Marcus Kempe, marcus.kempe@sp.se

License:
```
The MIT License (MIT)

Copyright (c) 2015 SP Technical Research Institute of Sweden

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
```
