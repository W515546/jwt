# Python API Test Suite

This repository contains a Python test suite for testing API endpoints using `unittest`. The test suite includes JWT authentication, requests to test endpoints, and assertions to validate the API's behavior.

## Requirements

To use this test suite, you need Python 3.8 or later, `pip` (Python package manager), and optionally `virtualenv` for managing dependencies in an isolated environment.

## Installation

To get started, ensure Python is installed on your system. You can download Python from the [official website](https://www.python.org/) and verify the installation by running `python --version` or `python3 --version`. If you don’t already have `virtualenv` installed, you can add it using `pip install virtualenv`. Once installed, create a virtual environment using `virtualenv venv` or `python3 -m venv venv`. Activate the virtual environment by running `venv\Scripts\activate` on Windows or `source venv/bin/activate` on Mac/Linux.

Next, install the dependencies required for the test suite. If a `requirements.txt` file is available, run `pip install -r requirements.txt`. If it’s not available, create one with the following dependencies: `requests` and `pyjwt`, then install the dependencies with the same command.

## Usage

To run the test suite, ensure your virtual environment is activated, then execute the following command: `python -m unittest <filename>.py`, replacing `<filename>` with the name of your test suite file. The test suite is configured to use a local API endpoint (`https://localhost:44323`). Make sure the API is running locally and accessible at this URL. SSL verification is disabled for local testing. You can modify the `verify=False` parameter in the requests calls if SSL verification is required.

## Customization

You can customize the JWT payload by creating a `payload_options.json` file in the root directory. For example:

```json
{
  "aud": "custom-audience",
  "iss": "https://custom-authority.com"
}
