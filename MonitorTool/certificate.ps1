New-SelfSignedCertificate -Type Custom -Subject "CN=YangDR, O=MechDancer, C=China" -KeyUsage DigitalSignature -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")