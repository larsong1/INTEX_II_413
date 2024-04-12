window.addEventListener("load", () => {
    const qrElement = document.getElementById("qrCode");
    const uri = document.getElementById("qrCodeData").getAttribute('data-url');

    if (!uri) {
        console.error('No URI found for QR code generation.');
        qrElement.textContent = 'Error: QR code cannot be generated because the URI data is missing.';
        return;
    }

    try {
        new QRCode(qrElement, {
            text: uri,
            width: 150,
            height: 150,
            colorDark: "#000000",
            colorLight: "#ffffff",
            correctLevel: QRCode.CorrectLevel.H
        });
    } catch (error) {
        console.error('Error generating QR code:', error);
        qrElement.textContent = 'Error: QR code cannot be generated due to an error.';
    }
});
