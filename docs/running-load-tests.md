# Running load tests

## Software requirements
- [k6](https://k6.io/docs/getting-started/installation/)
- node & npm

## Initial setup

### Install NPM packages
```shell
load-tests$ npm install
```

## Running a test

Each of the `.js` files in the `load-tests` folder has a test within in. Find the test you want to run and examine it to determine what environment variables it requires.
Use the `k6` CLI to run the test and provide the required environment variables.

### Example
`k6 run -e API_KEY="developer" -e HOST="https://qualified-teachers-api-dev.london.cloudapps.digital" -e TRN=2007946 -e BIRTH_DATE="1996-07-02" .\get-teacher.js`
