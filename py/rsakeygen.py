from cryptography.hazmat.primitives.asymmetric import rsa
from cryptography.hazmat.primitives import serialization
import base64


def generate_rsa_key():
    print(". Generating RSA key...")
    return rsa.generate_private_key(
        public_exponent=65537,
        key_size=2048
    )


def export_private_key_pem(private_key):
    return private_key.private_bytes(
        encoding=serialization.Encoding.PEM,
        format=serialization.PrivateFormat.PKCS8,
        encryption_algorithm=serialization.NoEncryption()
    ).decode('utf-8')


def export_public_key_pem(public_key):
    return public_key.public_bytes(
        encoding=serialization.Encoding.PEM,
        format=serialization.PublicFormat.SubjectPublicKeyInfo
    ).decode('utf-8')


def save_key_to_file(key_pem, file_name):
    with open(file_name, "w") as key_file:
        key_file.write(key_pem)
    print(f"Key saved to {file_name}")


def rsa_key_to_xml(key):
    if isinstance(key, rsa.RSAPrivateKey):
        private_numbers = key.private_numbers()
        public_numbers = private_numbers.public_numbers
        return f"""
<RSAKeyValue>
  <Modulus>{base64.b64encode(public_numbers.n.to_bytes((public_numbers.n.bit_length() + 7) // 8, 'big')).decode()}</Modulus>
  <Exponent>{base64.b64encode(public_numbers.e.to_bytes((public_numbers.e.bit_length() + 7) // 8, 'big')).decode()}</Exponent>
  <P>{base64.b64encode(private_numbers.p.to_bytes((private_numbers.p.bit_length() + 7) // 8, 'big')).decode()}</P>
  <Q>{base64.b64encode(private_numbers.q.to_bytes((private_numbers.q.bit_length() + 7) // 8, 'big')).decode()}</Q>
  <DP>{base64.b64encode(private_numbers.dmp1.to_bytes((private_numbers.dmp1.bit_length() + 7) // 8, 'big')).decode()}</DP>
  <DQ>{base64.b64encode(private_numbers.dmq1.to_bytes((private_numbers.dmq1.bit_length() + 7) // 8, 'big')).decode()}</DQ>
  <InverseQ>{base64.b64encode(private_numbers.iqmp.to_bytes((private_numbers.iqmp.bit_length() + 7) // 8, 'big')).decode()}</InverseQ>
  <D>{base64.b64encode(private_numbers.d.to_bytes((private_numbers.d.bit_length() + 7) // 8, 'big')).decode()}</D>
</RSAKeyValue>
"""
    elif isinstance(key, rsa.RSAPublicKey):
        public_numbers = key.public_numbers()
        return f"""
<RSAKeyValue>
  <Modulus>{base64.b64encode(public_numbers.n.to_bytes((public_numbers.n.bit_length() + 7) // 8, 'big')).decode()}</Modulus>
  <Exponent>{base64.b64encode(public_numbers.e.to_bytes((public_numbers.e.bit_length() + 7) // 8, 'big')).decode()}</Exponent>
</RSAKeyValue>
"""
    else:
        raise ValueError("Unsupported key type")


def generate_and_save_keys(key_name="default"):
    """save to pem and xml"""
    
    private_key = generate_rsa_key()
    public_key = private_key.public_key()

    private_key_pem = export_private_key_pem(private_key)
    public_key_pem = export_public_key_pem(public_key)

    private_key_filename = f"{key_name}_private_key.pem"
    public_key_filename = f"{key_name}_public_key.pem"

    save_key_to_file(private_key_pem, private_key_filename)
    save_key_to_file(public_key_pem, public_key_filename)

    private_key_xml = rsa_key_to_xml(private_key)
    public_key_xml = rsa_key_to_xml(public_key)

    private_key_xml_filename = f"{key_name}_private_key.xml"
    public_key_xml_filename = f"{key_name}_public_key.xml"

    save_key_to_file(private_key_xml, private_key_xml_filename)
    save_key_to_file(public_key_xml, public_key_xml_filename)


if __name__ == '__main__':
    print(".start")
    generate_and_save_keys('new_to_check')
    print("done.")


