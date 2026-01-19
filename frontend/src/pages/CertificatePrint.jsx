import { useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import api from '../services/api'
import './CertificatePrint.css'

const barangayInfo = {
  republic: 'REPUBLIC OF THE PHILIPPINES',
  province: 'Province of Batangas',
  municipality: 'Municipality of Lian',
  barangay: 'Barangay 5',
  address: 'San Juan St., Barangay 5, Lian Batangas',
  captain: 'HON. JASON VERGARA',
  secretary: 'ROSINDA R. CORPUZ',
}

export default function CertificatePrint() {
  const { id } = useParams()

  const { data: certificate, isLoading } = useQuery({
    queryKey: ['certificate-print', id],
    queryFn: () => api.get(`/certificates/${id}`).then((res) => res.data),
    enabled: !!id,
  })

  useEffect(() => {
    if (certificate) {
      const timer = setTimeout(() => window.print(), 600)
      return () => clearTimeout(timer)
    }
  }, [certificate])

  if (isLoading) {
    return (
      <div className="print-page">
        <p>Preparing certificate...</p>
      </div>
    )
  }

  if (!certificate) {
    return (
      <div className="print-page">
        <p>Certificate not found.</p>
      </div>
    )
  }

  const resident = certificate.resident || {}
  const birthDate = resident.dateOfBirth
    ? new Date(resident.dateOfBirth).toLocaleDateString('en-PH', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      })
    : 'N/A'

  const issueDate = certificate.issueDate
    ? new Date(certificate.issueDate).toLocaleDateString('en-PH', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      })
    : 'N/A'

  const qrImage =
    certificate.qrCodeData || certificate.qrCodeImagePath
      ? (certificate.qrCodeData || certificate.qrCodeImagePath || '').startsWith('data:')
        ? certificate.qrCodeData || certificate.qrCodeImagePath
        : `data:image/png;base64,${certificate.qrCodeData || certificate.qrCodeImagePath}`
      : null

  return (
    <div className="print-page">
      <div className="certificate-container">
        <div className="certificate-header">
          <div className="header-text">
            <p className="republic">{barangayInfo.republic}</p>
            <p>{barangayInfo.province}</p>
            <p>{barangayInfo.municipality}</p>
            <h1>{barangayInfo.barangay.toUpperCase()}</h1>
            <p className="address">{barangayInfo.address}</p>
          </div>
          <div className="header-photo">
            {resident.profilePhoto ? (
              <img src={resident.profilePhoto} alt="Barangay Official" />
            ) : (
              <div className="photo-placeholder">Barangay Official</div>
            )}
          </div>
        </div>

        <div className="certificate-title">
          <h2>BARANGAY CLEARANCE</h2>
        </div>

        <div className="certificate-body">
          <p className="salutation">To whom it may concern,</p>
          <p className="content-paragraph">
            This is to certify that{' '}
            <span className="highlight">
              {resident.firstName} {resident.middleName} {resident.lastName}
            </span>
            , {resident.civilStatus || 'Single'}, born on {birthDate}, resident of{' '}
            {resident.address || 'Barangay 5, Lian, Batangas'}.
          </p>
          <p className="content-paragraph">
            He/She is known to be a person with good moral character and has been living in our
            barangay. He/She was not convicted nor has any derogatory record filed in this barangay.
          </p>
          {certificate.purpose && (
            <p className="content-paragraph">
              This certification is being issued upon the request of the above-named person for{' '}
              <span className="highlight">{certificate.purpose}</span>.
            </p>
          )}
          <p className="content-paragraph">
            Issued this {issueDate} at Barangay 5, Lian Batangas.
          </p>
        </div>

        <div className="signature-section">
          <div className="signature-block">
            <div className="signature-line" />
            <p className="signature-name">{barangayInfo.captain}</p>
            <p className="signature-title">Punong Barangay</p>
          </div>
          <div className="signature-block">
            <div className="signature-line" />
            <p className="signature-name">{barangayInfo.secretary}</p>
            <p className="signature-title">Barangay Secretary</p>
          </div>
        </div>

        <div className="details-section">
          <p>
            <strong>Certificate No.:</strong> {certificate.certificateNumber || 'N/A'}
          </p>
          <p>
            <strong>Issued at:</strong> {barangayInfo.barangay.toUpperCase()}, {barangayInfo.municipality}
          </p>
        </div>

        <div className="footer-section">
          {qrImage && (
            <div className="qr-wrapper">
              <img src={qrImage} alt="QR Code" />
              <span>Scan to verify</span>
            </div>
          )}
          <div className="footer-note">
            <p>"Malaasakit at pagmamahal para sa inyo ay para sa inyo"</p>
          </div>
        </div>
      </div>
    </div>
  )
}




