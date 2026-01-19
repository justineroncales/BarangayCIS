import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import api from "../services/api";
import {
  Search,
  Plus,
  Edit,
  Trash2,
  User,
  Stethoscope,
  MapPin,
  BookOpen,
  FileText,
  BarChart3,
  Download,
} from "lucide-react";
import { toast } from "react-hot-toast";
import BHWProfileModal from "../components/BHWProfileModal";
import BHWAssignmentModal from "../components/BHWAssignmentModal";
import BHWVisitLogModal from "../components/BHWVisitLogModal";
import BHWTrainingModal from "../components/BHWTrainingModal";
import DeliveryModal from "../components/DeliveryModal";
import KRAReportModal from "../components/KRAReportModal";
import Pagination from "../components/Pagination";
import "./Pages.css";

export default function BHW() {
  const [activeTab, setActiveTab] = useState("profiles");
  const [search, setSearch] = useState("");
  const [showModal, setShowModal] = useState(false);
  const [editingItem, setEditingItem] = useState(null);
  const [selectedBHWId, setSelectedBHWId] = useState(null);
  const [reportSearch, setReportSearch] = useState("");
  const [reportPage, setReportPage] = useState(1);
  const [reportPageSize, setReportPageSize] = useState(10);
  const [showAgeDistribution, setShowAgeDistribution] = useState(false);
  const [showWRAReport, setShowWRAReport] = useState(false);
  const [showAnnualCatchment, setShowAnnualCatchment] = useState(false);
  const [showPopAgeIndividual, setShowPopAgeIndividual] = useState(false);
  const [reportBHWId, setReportBHWId] = useState("all");
  const [reportQuarter, setReportQuarter] = useState("4TH");
  const [reportYear, setReportYear] = useState(new Date().getFullYear());
  const [reportMonth, setReportMonth] = useState(new Date().getMonth() + 1);
  const [submittedBy, setSubmittedBy] = useState("");
  const queryClient = useQueryClient();

  const tabs = [
    { id: "profiles", label: "BHW Profiles", icon: User },
    { id: "assignments", label: "Assignments", icon: MapPin },
    { id: "visits", label: "Visit Logs", icon: Stethoscope },
    { id: "trainings", label: "Trainings", icon: BookOpen },
    { id: "deliveries", label: "Deliveries", icon: FileText },
    { id: "kra-reports", label: "KRA Reports", icon: BarChart3 },
    { id: "reports", label: "Resident Reports", icon: FileText },
  ];

  // Profiles
  const { data: profiles = [] } = useQuery({
    queryKey: ["bhw-profiles", search],
    queryFn: () =>
      api
        .get("/bhw-profiles", { params: { search, status: "" } })
        .then((res) => res.data),
    enabled: activeTab === "profiles",
  });

  // Assignments
  const { data: assignments = [] } = useQuery({
    queryKey: ["bhw-assignments"],
    queryFn: () => api.get("/bhw-assignments").then((res) => res.data),
    enabled: activeTab === "assignments",
  });

  // Visit Logs
  const { data: visits = [] } = useQuery({
    queryKey: ["bhw-visit-logs"],
    queryFn: () => api.get("/bhw-visit-logs").then((res) => res.data),
    enabled: activeTab === "visits",
  });

  // Trainings
  const { data: trainings = [] } = useQuery({
    queryKey: ["bhw-trainings"],
    queryFn: () => api.get("/bhw-trainings").then((res) => res.data),
    enabled: activeTab === "trainings",
  });

  // Deliveries
  const { data: deliveries = [] } = useQuery({
    queryKey: ["deliveries", reportYear],
    queryFn: () =>
      api
        .get("/bhw-reports/deliveries", { params: { year: reportYear } })
        .then((res) => res.data),
    enabled: activeTab === "deliveries",
  });

  // KRA Reports
  const { data: kraReports = [] } = useQuery({
    queryKey: ["kra-reports", reportYear],
    queryFn: () =>
      api
        .get("/bhw-reports/kra", { params: { year: reportYear } })
        .then((res) => res.data),
    enabled: activeTab === "kra-reports",
  });

  // BHW Reports - Get active BHWs for selection
  const { data: activeBHWs = [] } = useQuery({
    queryKey: ["bhw-profiles-active"],
    queryFn: () =>
      api
        .get("/bhw-profiles", { params: { status: "Active" } })
        .then((res) => res.data),
    enabled: activeTab === "reports",
  });

  // Get residents for selected BHW (paginated)
  const { data: bhwResidentsPaged, isLoading: isLoadingBHWResidents } = useQuery({
    queryKey: ["residents-by-bhw", selectedBHWId, reportSearch, reportPage, reportPageSize],
    queryFn: () =>
      api
        .get(`/residents/by-bhw/${selectedBHWId}`, {
          params: { search: reportSearch, page: reportPage, pageSize: reportPageSize },
        })
        .then((res) => res.data),
    enabled: activeTab === "reports" && selectedBHWId !== null,
  });

  const bhwResidents = bhwResidentsPaged?.data || [];
  const bhwResidentsTotalPages = bhwResidentsPaged?.totalPages || 0;
  const bhwResidentsTotalCount = bhwResidentsPaged?.totalCount || 0;

  // Reset to page 1 when search or BHW changes
  const handleReportSearchChange = (value) => {
    setReportSearch(value);
    setReportPage(1);
  };

  // Get statistics for selected BHW
  const { data: bhwStatistics = null } = useQuery({
    queryKey: ["bhw-statistics", selectedBHWId],
    queryFn: () =>
      api
        .get(`/residents/bhw/${selectedBHWId}/statistics`)
        .then((res) => res.data),
    enabled: activeTab === "reports" && selectedBHWId !== null,
  });

  const deleteMutation = useMutation({
    mutationFn: ({ type, id }) => {
      const endpoints = {
        profiles: "/bhw-profiles",
        assignments: "/bhw-assignments",
        visits: "/bhw-visit-logs",
        trainings: "/bhw-trainings",
        deliveries: "/bhw-reports/deliveries",
        "kra-reports": "/bhw-reports/kra",
      };
      return api.delete(`${endpoints[type]}/${id}`);
    },
    onSuccess: (_, variables) => {
      if (variables.type === "deliveries") {
        queryClient.invalidateQueries(["deliveries"]);
      } else if (variables.type === "kra-reports") {
        queryClient.invalidateQueries(["kra-reports"]);
      } else {
        queryClient.invalidateQueries([`bhw-${variables.type}`]);
      }
      toast.success("Item deleted successfully");
    },
  });

  const handleDelete = (type, id) => {
    if (window.confirm("Are you sure you want to delete this item?")) {
      deleteMutation.mutate({ type, id });
    }
  };

  const generateAgeDistributionReport = () => {
    const params = new URLSearchParams({
      quarter: reportQuarter,
      year: reportYear.toString(),
    });

    if (reportBHWId !== "all") {
      params.append("bhwProfileId", reportBHWId);
    }

    if (submittedBy) {
      params.append("submittedBy", submittedBy);
    }

    const url = `${api.defaults.baseURL || "http://localhost:5000/api"}/bhw-profiles/population-age-distribution/print?${params.toString()}`;
    window.open(url, "_blank");
  };

  const generateWRAReport = () => {
    const params = new URLSearchParams({
      month: reportMonth.toString(),
      year: reportYear.toString(),
    });

    if (reportBHWId !== "all") {
      params.append("bhwProfileId", reportBHWId);
    }

    if (submittedBy) {
      params.append("submittedBy", submittedBy);
    }

    const url = `${api.defaults.baseURL || "http://localhost:5000/api"}/bhw-profiles/wra-report/print?${params.toString()}`;
    window.open(url, "_blank");
  };

  const renderProfiles = () => (
    <div className="table-container-wrapper">
      <div className="table-container">
        <table className="data-table">
          <thead>
            <tr>
              <th>BHW Number</th>
              <th>Name</th>
              <th>Contact</th>
              <th>Zone Assignment</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {profiles.length === 0 ? (
              <tr>
                <td
                  colSpan="6"
                  style={{ textAlign: "center", padding: "2rem" }}
                >
                  No BHW profiles found. Click "Add" to create one.
                </td>
              </tr>
            ) : (
              profiles.map((profile) => (
                <tr key={profile.id}>
                  <td>{profile.bhwNumber}</td>
                  <td>
                    {profile.firstName} {profile.lastName}
                  </td>
                  <td>{profile.contactNumber || "-"}</td>
                  <td>
                    {(() => {
                      const activeAssignment = profile.assignments?.find(
                        (a) => a && a.status === "Active"
                      );
                      return activeAssignment ? activeAssignment.zoneName : "-";
                    })()}
                  </td>
                  <td>
                    <span
                      className={`badge ${
                        profile.status === "Active"
                          ? "badge-success"
                          : "badge-secondary"
                      }`}
                    >
                      {profile.status}
                    </span>
                  </td>
                  <td>
                    <div className="action-buttons">
                      <button
                        onClick={() => {
                          setEditingItem(profile);
                          setShowModal(true);
                        }}
                      >
                        <Edit size={16} />
                      </button>
                      <button
                        onClick={() => handleDelete("profiles", profile.id)}
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );

  const renderAssignments = () => (
    <div className="table-container-wrapper">
      <div className="table-container">
        <table className="data-table">
          <thead>
            <tr>
              <th>BHW Name</th>
              <th>Zone</th>
              <th>Assignment Date</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {assignments.length === 0 ? (
              <tr>
                <td
                  colSpan="5"
                  style={{ textAlign: "center", padding: "2rem" }}
                >
                  No assignments found.
                </td>
              </tr>
            ) : (
              assignments.map((assignment) => (
                <tr key={assignment.id}>
                  <td>
                    {assignment.bhwProfile?.firstName}{" "}
                    {assignment.bhwProfile?.lastName}
                  </td>
                  <td>{assignment.zoneName}</td>
                  <td>
                    {new Date(assignment.assignmentDate).toLocaleDateString()}
                  </td>
                  <td>
                    <span
                      className={`badge ${
                        assignment.status === "Active"
                          ? "badge-success"
                          : "badge-secondary"
                      }`}
                    >
                      {assignment.status}
                    </span>
                  </td>
                  <td>
                    <div className="action-buttons">
                      <button
                        onClick={() => {
                          setEditingItem(assignment);
                          setShowModal(true);
                        }}
                      >
                        <Edit size={16} />
                      </button>
                      <button
                        onClick={() =>
                          handleDelete("assignments", assignment.id)
                        }
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );

  const renderVisits = () => (
    <div className="table-container-wrapper">
      <div className="table-container">
        <table className="data-table">
          <thead>
            <tr>
              <th>BHW</th>
              <th>Visited Person</th>
              <th>Visit Date</th>
              <th>Visit Type</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {visits.length === 0 ? (
              <tr>
                <td
                  colSpan="5"
                  style={{ textAlign: "center", padding: "2rem" }}
                >
                  No visit logs found.
                </td>
              </tr>
            ) : (
              visits.map((visit) => (
                <tr key={visit.id}>
                  <td>
                    {visit.bhwProfile?.firstName} {visit.bhwProfile?.lastName}
                  </td>
                  <td>
                    {visit.resident
                      ? `${visit.resident.firstName} ${visit.resident.lastName}`
                      : visit.visitedPersonName || "-"}
                  </td>
                  <td>{new Date(visit.visitDate).toLocaleDateString()}</td>
                  <td>{visit.visitType}</td>
                  <td>
                    <div className="action-buttons">
                      <button
                        onClick={() => {
                          setEditingItem(visit);
                          setShowModal(true);
                        }}
                      >
                        <Edit size={16} />
                      </button>
                      <button onClick={() => handleDelete("visits", visit.id)}>
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );

  const renderTrainings = () => (
    <div className="table-container-wrapper">
      <div className="table-container">
        <table className="data-table">
          <thead>
            <tr>
              <th>BHW</th>
              <th>Training Title</th>
              <th>Provider</th>
              <th>Date</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {trainings.length === 0 ? (
              <tr>
                <td
                  colSpan="6"
                  style={{ textAlign: "center", padding: "2rem" }}
                >
                  No trainings found.
                </td>
              </tr>
            ) : (
              trainings.map((training) => (
                <tr key={training.id}>
                  <td>
                    {training.bhwProfile?.firstName}{" "}
                    {training.bhwProfile?.lastName}
                  </td>
                  <td>{training.trainingTitle}</td>
                  <td>{training.trainingProvider || "-"}</td>
                  <td>
                    {new Date(training.trainingDate).toLocaleDateString()}
                  </td>
                  <td>
                    <span
                      className={`badge ${
                        training.status === "Completed"
                          ? "badge-success"
                          : "badge-secondary"
                      }`}
                    >
                      {training.status}
                    </span>
                  </td>
                  <td>
                    <div className="action-buttons">
                      <button
                        onClick={() => {
                          setEditingItem(training);
                          setShowModal(true);
                        }}
                      >
                        <Edit size={16} />
                      </button>
                      <button
                        onClick={() => handleDelete("trainings", training.id)}
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );

  const renderDeliveries = () => (
    <div>
      <div className="filter-controls">
        <div
          style={{
            display: "flex",
            gap: "1rem",
            alignItems: "center",
            flex: 1,
          }}
        >
          <label style={{ fontWeight: 500, whiteSpace: "nowrap" }}>
            Filter by Year:
          </label>
          <input
            type="number"
            value={reportYear}
            onChange={(e) =>
              setReportYear(
                parseInt(e.target.value) || new Date().getFullYear()
              )
            }
            min="2020"
            max="2100"
            style={{
              padding: "0.5rem 0.75rem",
              borderRadius: "6px",
              border: "1px solid var(--border-color)",
              width: "120px",
              background: "var(--bg-primary)",
            }}
          />
        </div>
        <button
          className="btn-secondary"
          onClick={() => {
            const url = `${
              api.defaults.baseURL || "http://localhost:5000"
            }/api/bhw-reports/deliveries/template/download`;
            window.open(url, "_blank");
          }}
          style={{
            display: "inline-flex",
            alignItems: "center",
            gap: "0.5rem",
            whiteSpace: "nowrap",
          }}
        >
          <Download size={18} />
          Download Template
        </button>
      </div>
      <div className="table-container-wrapper">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>No</th>
                <th>Mother Name</th>
                <th>Child Name</th>
                <th>Purok/Sitio</th>
                <th>Gender</th>
                <th>Date of Birth</th>
                <th>Time</th>
                <th>Weight</th>
                <th>Height</th>
                <th>Place of Birth</th>
                <th>Delivery Type</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {deliveries.length === 0 ? (
                <tr>
                  <td
                    colSpan="12"
                    style={{ textAlign: "center", padding: "2rem" }}
                  >
                    No deliveries found for {reportYear}. Click "Add" to create
                    one.
                  </td>
                </tr>
              ) : (
                deliveries.map((delivery, index) => (
                  <tr key={delivery.id}>
                    <td>{index + 1}</td>
                    <td>{delivery.motherName}</td>
                    <td>{delivery.childName}</td>
                    <td>{delivery.purokSitio || "-"}</td>
                    <td>{delivery.gender}</td>
                    <td>
                      {new Date(delivery.dateOfBirth).toLocaleDateString()}
                    </td>
                    <td>{delivery.timeOfBirth || "-"}</td>
                    <td>{delivery.weight || "-"}</td>
                    <td>{delivery.height || "-"}</td>
                    <td>{delivery.placeOfBirth || "-"}</td>
                    <td>{delivery.deliveryType || "-"}</td>
                    <td>
                      <div className="action-buttons">
                        <button
                          onClick={() => {
                            setEditingItem(delivery);
                            setShowModal(true);
                          }}
                        >
                          <Edit size={16} />
                        </button>
                        <button
                          onClick={() =>
                            handleDelete("deliveries", delivery.id)
                          }
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );

  const renderKRAReports = () => (
    <div>
      <div className="filter-controls">
        <div
          style={{
            display: "flex",
            gap: "1rem",
            alignItems: "center",
            flex: 1,
          }}
        >
          <label style={{ fontWeight: 500, whiteSpace: "nowrap" }}>
            Filter by Year:
          </label>
          <input
            type="number"
            value={reportYear}
            onChange={(e) =>
              setReportYear(
                parseInt(e.target.value) || new Date().getFullYear()
              )
            }
            min="2020"
            max="2100"
            style={{
              padding: "0.5rem 0.75rem",
              borderRadius: "6px",
              border: "1px solid var(--border-color)",
              width: "120px",
              background: "var(--bg-primary)",
            }}
          />
        </div>
        <button
          className="btn-secondary"
          onClick={() => {
            const url = `${
              api.defaults.baseURL || "http://localhost:5000"
            }/api/bhw-reports/kra/template/download`;
            window.open(url, "_blank");
          }}
          style={{
            display: "inline-flex",
            alignItems: "center",
            gap: "0.5rem",
            whiteSpace: "nowrap",
          }}
        >
          <Download size={18} />
          Download Template
        </button>
      </div>
      <div className="table-container-wrapper">
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>BHW</th>
                <th>Year</th>
                <th>Month</th>
                <th>PILLS-COC (20+)</th>
                <th>DMPA (20+)</th>
                <th>CONDOM (20+)</th>
                <th>IMPLANT (20+)</th>
                <th>IUD (20+)</th>
                <th>DELIVERIES (20+)</th>
                <th>Teenage Pregnancies</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {kraReports.length === 0 ? (
                <tr>
                  <td
                    colSpan="11"
                    style={{ textAlign: "center", padding: "2rem" }}
                  >
                    No KRA reports found for {reportYear}. Click "Add" to create
                    one.
                  </td>
                </tr>
              ) : (
                kraReports.map((report) => {
                  const monthNames = [
                    "January",
                    "February",
                    "March",
                    "April",
                    "May",
                    "June",
                    "July",
                    "August",
                    "September",
                    "October",
                    "November",
                    "December",
                  ];
                  return (
                    <tr key={report.id}>
                      <td>
                        {report.bhwProfile?.firstName}{" "}
                        {report.bhwProfile?.lastName}
                      </td>
                      <td>{report.year}</td>
                      <td>{monthNames[report.month - 1]}</td>
                      <td>{report.pillsCOC_20Plus}</td>
                      <td>{report.dmpA_20Plus ?? report.dMPA_20Plus ?? 0}</td>
                      <td>{report.condom_20Plus}</td>
                      <td>{report.implant_20Plus}</td>
                      <td>{report.iud_20Plus}</td>
                      <td>{report.deliveries_20Plus}</td>
                      <td>{report.teenagePregnancies}</td>
                      <td>
                        <div className="action-buttons">
                          <button
                            onClick={() => {
                              setEditingItem(report);
                              setShowModal(true);
                            }}
                          >
                            <Edit size={16} />
                          </button>
                          <button
                            onClick={() =>
                              handleDelete("kra-reports", report.id)
                            }
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );

  const renderReports = () => (
    <div>
      {!showAgeDistribution && !showWRAReport && !showAnnualCatchment && !showPopAgeIndividual && !selectedBHWId ? (
        <div>
          <div className="report-card">
            <h2>
              <BarChart3 size={24} />
              Population Age Distribution Report
            </h2>
            <p>
              Generate a population age distribution report for all BHWs or a
              specific BHW assignment.
            </p>
            <button
              className="btn-primary"
              onClick={() => setShowAgeDistribution(true)}
              style={{
                display: "inline-flex",
                alignItems: "center",
                gap: "0.5rem",
              }}
            >
              <Download size={18} />
              Generate Population Age Distribution Report
            </button>
          </div>

          <div className="report-card">
            <h2>
              <BarChart3 size={24} />
              Annual Catchment Population Summary Report
            </h2>
            <p>
              Generate an annual catchment population summary report with age groups:
              Less than 1 YR., 1-4, 5-6, 7-14, 15-49, 50-64, 65-OVER.
            </p>
            <button
              className="btn-primary"
              onClick={() => setShowAnnualCatchment(true)}
              style={{
                display: "inline-flex",
                alignItems: "center",
                gap: "0.5rem",
              }}
            >
              <Download size={18} />
              Generate Annual Catchment Report
            </button>
          </div>

          <div className="report-card">
            <h2>
              <BarChart3 size={24} />
              POP AGE INDIVIDUAL Report
            </h2>
            <p>
              Generate a detailed individual age distribution report showing population
              by individual ages from 0-5 months to 88+ years.
            </p>
            <button
              className="btn-primary"
              onClick={() => setShowPopAgeIndividual(true)}
              style={{
                display: "inline-flex",
                alignItems: "center",
                gap: "0.5rem",
              }}
            >
              <Download size={18} />
              Generate POP AGE INDIVIDUAL Report
            </button>
          </div>

          <div className="report-card">
            <h2>
              <BarChart3 size={24} />
              Women of Reproductive Age (WRA) Report
            </h2>
            <p>
              Generate an FHSIS report for Women of Reproductive Age (10-49
              years old) by age groups.
            </p>
            <button
              className="btn-primary"
              onClick={() => setShowWRAReport(true)}
              style={{
                display: "inline-flex",
                alignItems: "center",
                gap: "0.5rem",
              }}
            >
              <Download size={18} />
              Generate WRA Report
            </button>
          </div>

          <div className="section-card" style={{ textAlign: "center" }}>
            <h2 style={{ marginBottom: "1rem" }}>
              Select a BHW to view resident reports
            </h2>
            <div
              style={{
                display: "grid",
                gridTemplateColumns: "repeat(auto-fill, minmax(250px, 1fr))",
                gap: "1rem",
                marginTop: "2rem",
              }}
            >
              {activeBHWs.map((bhw) => (
                <div
                  key={bhw.id}
                  onClick={() => setSelectedBHWId(bhw.id)}
                  style={{
                    padding: "1.5rem",
                    border: "1px solid var(--border)",
                    borderRadius: "8px",
                    cursor: "pointer",
                    transition: "all 0.2s",
                    background: "var(--bg-primary)",
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.background = "var(--bg-secondary)";
                    e.currentTarget.style.transform = "translateY(-2px)";
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.background = "var(--bg-primary)";
                    e.currentTarget.style.transform = "translateY(0)";
                  }}
                >
                  <div style={{ fontWeight: 600, marginBottom: "0.5rem" }}>
                    {bhw.firstName} {bhw.lastName}
                  </div>
                  <div
                    style={{
                      fontSize: "0.875rem",
                      color: "var(--text-secondary)",
                      marginBottom: "0.5rem",
                    }}
                  >
                    {bhw.bhwNumber}
                  </div>
                  {bhw.assignments?.find((a) => a && a.status === "Active") && (
                    <div
                      style={{
                        fontSize: "0.75rem",
                        color: "var(--text-secondary)",
                      }}
                    >
                      Zone:{" "}
                      {
                        bhw.assignments.find((a) => a && a.status === "Active")
                          .zoneName
                      }
                    </div>
                  )}
                </div>
              ))}
            </div>
            {activeBHWs.length === 0 && (
              <p style={{ color: "var(--text-secondary)" }}>
                No active BHWs found.
              </p>
            )}
          </div>
        </div>
      ) : showAnnualCatchment ? (
        <div>
          <div className="section-header">
            <h2>Generate Annual Catchment Population Summary Report</h2>
            <button
              onClick={() => {
                setShowAnnualCatchment(false);
                setReportBHWId("all");
                setReportYear(new Date().getFullYear());
                setSubmittedBy("");
              }}
              className="btn-secondary"
            >
              ← Back
            </button>
          </div>

          <div
            className="section-card"
            style={{ maxWidth: "700px", margin: "0 auto" }}
          >
            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                BHW Assignment
              </label>
              <select
                value={reportBHWId}
                onChange={(e) => setReportBHWId(e.target.value)}
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              >
                <option value="all">All BHWs</option>
                {activeBHWs.map((bhw) => (
                  <option key={bhw.id} value={bhw.id}>
                    {bhw.firstName} {bhw.lastName} ({bhw.bhwNumber})
                  </option>
                ))}
              </select>
            </div>

            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                Year
              </label>
              <input
                type="number"
                value={reportYear}
                onChange={(e) =>
                  setReportYear(
                    parseInt(e.target.value) || new Date().getFullYear()
                  )
                }
                min="2020"
                max="2100"
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              />
            </div>

            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                Submitted By (Optional)
              </label>
              <input
                type="text"
                value={submittedBy}
                onChange={(e) => setSubmittedBy(e.target.value)}
                placeholder="BHW Name"
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              />
            </div>

            <button
              className="btn-primary"
              onClick={() => {
                const params = new URLSearchParams({
                  year: reportYear.toString(),
                });
                if (reportBHWId !== "all") {
                  params.append("bhwProfileId", reportBHWId);
                }
                if (submittedBy) {
                  params.append("submittedBy", submittedBy);
                }
                const url = `${api.defaults.baseURL || "http://localhost:5000/api"}/bhw-profiles/annual-catchment-population/print?${params.toString()}`;
                window.open(url, "_blank");
              }}
              style={{
                width: "100%",
                display: "inline-flex",
                alignItems: "center",
                justifyContent: "center",
                gap: "0.5rem",
              }}
            >
              <Download size={18} />
              Generate & Print Annual Catchment Report
            </button>
          </div>
        </div>
      ) : showPopAgeIndividual ? (
        <div>
          <div className="section-header">
            <h2>Generate POP AGE INDIVIDUAL Report</h2>
            <button
              onClick={() => {
                setShowPopAgeIndividual(false);
                setReportBHWId("all");
                setReportYear(new Date().getFullYear());
                setSubmittedBy("");
              }}
              className="btn-secondary"
            >
              ← Back
            </button>
          </div>

          <div
            className="section-card"
            style={{ maxWidth: "700px", margin: "0 auto" }}
          >
            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                BHW Assignment
              </label>
              <select
                value={reportBHWId}
                onChange={(e) => setReportBHWId(e.target.value)}
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              >
                <option value="all">All BHWs</option>
                {activeBHWs.map((bhw) => (
                  <option key={bhw.id} value={bhw.id}>
                    {bhw.firstName} {bhw.lastName} ({bhw.bhwNumber})
                  </option>
                ))}
              </select>
            </div>

            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                Year
              </label>
              <input
                type="number"
                value={reportYear}
                onChange={(e) =>
                  setReportYear(
                    parseInt(e.target.value) || new Date().getFullYear()
                  )
                }
                min="2020"
                max="2100"
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              />
            </div>

            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                Submitted By (Optional)
              </label>
              <input
                type="text"
                value={submittedBy}
                onChange={(e) => setSubmittedBy(e.target.value)}
                placeholder="BHW Name"
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              />
            </div>

            <button
              className="btn-primary"
              onClick={() => {
                const params = new URLSearchParams({
                  year: reportYear.toString(),
                });
                if (reportBHWId !== "all") {
                  params.append("bhwProfileId", reportBHWId);
                }
                if (submittedBy) {
                  params.append("submittedBy", submittedBy);
                }
                const url = `${api.defaults.baseURL || "http://localhost:5000/api"}/bhw-profiles/pop-age-individual/print?${params.toString()}`;
                window.open(url, "_blank");
              }}
              style={{
                width: "100%",
                display: "inline-flex",
                alignItems: "center",
                justifyContent: "center",
                gap: "0.5rem",
              }}
            >
              <Download size={18} />
              Generate & Print POP AGE INDIVIDUAL Report
            </button>
          </div>
        </div>
      ) : showWRAReport ? (
        <div>
          <div className="section-header">
            <h2>Generate Women of Reproductive Age (WRA) Report</h2>
            <button
              onClick={() => {
                setShowWRAReport(false);
                setReportBHWId("all");
                setReportMonth(new Date().getMonth() + 1);
                setReportYear(new Date().getFullYear());
                setSubmittedBy("");
              }}
              className="btn-secondary"
            >
              ← Back
            </button>
          </div>

          <div
            className="section-card"
            style={{ maxWidth: "700px", margin: "0 auto" }}
          >
            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                BHW Assignment
              </label>
              <select
                value={reportBHWId}
                onChange={(e) => setReportBHWId(e.target.value)}
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              >
                <option value="all">All BHWs</option>
                {activeBHWs.map((bhw) => (
                  <option key={bhw.id} value={bhw.id}>
                    {bhw.firstName} {bhw.lastName} ({bhw.bhwNumber})
                  </option>
                ))}
              </select>
            </div>

            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                Month
              </label>
              <select
                value={reportMonth}
                onChange={(e) => setReportMonth(parseInt(e.target.value))}
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              >
                <option value="1">January</option>
                <option value="2">February</option>
                <option value="3">March</option>
                <option value="4">April</option>
                <option value="5">May</option>
                <option value="6">June</option>
                <option value="7">July</option>
                <option value="8">August</option>
                <option value="9">September</option>
                <option value="10">October</option>
                <option value="11">November</option>
                <option value="12">December</option>
              </select>
            </div>

            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                Year
              </label>
              <input
                type="number"
                value={reportYear}
                onChange={(e) =>
                  setReportYear(
                    parseInt(e.target.value) || new Date().getFullYear()
                  )
                }
                min="2020"
                max="2100"
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              />
            </div>

            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                Submitted By (Optional)
              </label>
              <input
                type="text"
                value={submittedBy}
                onChange={(e) => setSubmittedBy(e.target.value)}
                placeholder="BHW Name"
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              />
            </div>

            <button
              className="btn-primary"
              onClick={generateWRAReport}
              style={{
                width: "100%",
                display: "inline-flex",
                alignItems: "center",
                justifyContent: "center",
                gap: "0.5rem",
              }}
            >
              <Download size={18} />
              Generate & Print WRA Report
            </button>
          </div>
        </div>
      ) : showAgeDistribution ? (
        <div>
          <div className="section-header">
            <h2>Generate Population Age Distribution Report</h2>
            <button
              onClick={() => {
                setShowAgeDistribution(false);
                setReportBHWId("all");
                setReportQuarter("4TH");
                setReportYear(new Date().getFullYear());
                setSubmittedBy("");
              }}
              className="btn-secondary"
            >
              ← Back
            </button>
          </div>

          <div
            className="section-card"
            style={{ maxWidth: "700px", margin: "0 auto" }}
          >
            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                BHW Assignment
              </label>
              <select
                value={reportBHWId}
                onChange={(e) => setReportBHWId(e.target.value)}
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              >
                <option value="all">All BHWs</option>
                {activeBHWs.map((bhw) => (
                  <option key={bhw.id} value={bhw.id}>
                    {bhw.firstName} {bhw.lastName} ({bhw.bhwNumber})
                  </option>
                ))}
              </select>
            </div>

            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                Quarter
              </label>
              <select
                value={reportQuarter}
                onChange={(e) => setReportQuarter(e.target.value)}
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              >
                <option value="1ST">1st Quarter</option>
                <option value="2ND">2nd Quarter</option>
                <option value="3RD">3rd Quarter</option>
                <option value="4TH">4th Quarter</option>
              </select>
            </div>

            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                Year
              </label>
              <input
                type="number"
                value={reportYear}
                onChange={(e) =>
                  setReportYear(
                    parseInt(e.target.value) || new Date().getFullYear()
                  )
                }
                min="2020"
                max="2100"
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              />
            </div>

            <div style={{ marginBottom: "1.5rem" }}>
              <label
                style={{
                  display: "block",
                  marginBottom: "0.5rem",
                  fontWeight: 500,
                }}
              >
                Submitted By (Optional)
              </label>
              <input
                type="text"
                value={submittedBy}
                onChange={(e) => setSubmittedBy(e.target.value)}
                placeholder="BHW Name"
                style={{
                  width: "100%",
                  padding: "0.75rem",
                  borderRadius: "4px",
                  border: "1px solid var(--border)",
                  background: "var(--bg-primary)",
                }}
              />
            </div>

            <button
              className="btn-primary"
              onClick={generateAgeDistributionReport}
              style={{
                width: "100%",
                display: "inline-flex",
                alignItems: "center",
                justifyContent: "center",
                gap: "0.5rem",
              }}
            >
              <Download size={18} />
              Generate & Print Report
            </button>
          </div>
        </div>
      ) : (
        <div>
          <div className="section-header">
            <h2>
              {activeBHWs.find((b) => b.id === selectedBHWId)?.firstName}{" "}
              {activeBHWs.find((b) => b.id === selectedBHWId)?.lastName} -
              Resident Reports
            </h2>
            <button
              onClick={() => {
                setSelectedBHWId(null);
                setReportSearch("");
                setReportPage(1);
              }}
              className="btn-secondary"
            >
              ← Back to BHW Selection
            </button>
          </div>

          {bhwStatistics && (
            <div className="stats-grid">
              <div className="stat-card">
                <div className="stat-card-label">Total Residents</div>
                <div className="stat-card-value">
                  {bhwStatistics.totalResidents}
                </div>
              </div>
              <div className="stat-card">
                <div className="stat-card-label">Senior Citizens</div>
                <div className="stat-card-value">
                  {bhwStatistics.specialCategories?.seniorCitizens || 0}
                </div>
              </div>
              <div className="stat-card">
                <div className="stat-card-label">PWD</div>
                <div className="stat-card-value">
                  {bhwStatistics.specialCategories?.PWD || 0}
                </div>
              </div>
              <div className="stat-card">
                <div className="stat-card-label">Voters</div>
                <div className="stat-card-value">
                  {bhwStatistics.specialCategories?.voters || 0}
                </div>
              </div>
            </div>
          )}

          <div className="search-bar" style={{ marginBottom: "1rem" }}>
            <Search size={20} />
            <input
              type="text"
              placeholder="Search residents..."
              value={reportSearch}
              onChange={(e) => handleReportSearchChange(e.target.value)}
            />
            <select
              value={reportPageSize}
              onChange={(e) => {
                setReportPageSize(Number(e.target.value));
                setReportPage(1);
              }}
              style={{
                marginLeft: "1rem",
                padding: "0.5rem",
                border: "1px solid var(--border-color)",
                borderRadius: "6px",
                background: "var(--bg)",
                color: "var(--text)",
                cursor: "pointer",
              }}
            >
              <option value={10}>10 per page</option>
              <option value={25}>25 per page</option>
              <option value={50}>50 per page</option>
              <option value={100}>100 per page</option>
            </select>
          </div>

          <div className="table-container-wrapper">
            <div className="table-container">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Age</th>
                    <th>Gender</th>
                    <th>Address</th>
                    <th>Contact</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {bhwResidents.length === 0 ? (
                    <tr>
                      <td
                        colSpan="6"
                        style={{ textAlign: "center", padding: "2rem" }}
                      >
                        {selectedBHWId
                          ? "No residents assigned to this BHW."
                          : "Select a BHW to view residents."}
                      </td>
                    </tr>
                  ) : (
                    bhwResidents.map((resident) => {
                      const age = Math.floor(
                        (new Date() - new Date(resident.dateOfBirth)) /
                          (365.25 * 24 * 60 * 60 * 1000)
                      );
                      return (
                        <tr key={resident.id}>
                          <td>
                            {resident.firstName}{" "}
                            {resident.middleName
                              ? resident.middleName + " "
                              : ""}
                            {resident.lastName} {resident.suffix || ""}
                          </td>
                          <td>{age} years</td>
                          <td>{resident.gender}</td>
                          <td>{resident.address}</td>
                          <td>{resident.contactNumber || "-"}</td>
                          <td>
                            <div
                              style={{
                                display: "flex",
                                gap: "0.25rem",
                                flexWrap: "wrap",
                              }}
                            >
                              {resident.isVoter && (
                                <span
                                  className="badge badge-success"
                                  style={{ fontSize: "0.7rem" }}
                                >
                                  Voter
                                </span>
                              )}
                              {resident.isPWD && (
                                <span
                                  className="badge"
                                  style={{
                                    fontSize: "0.7rem",
                                    background: "rgba(59, 130, 246, 0.1)",
                                    color: "var(--accent)",
                                  }}
                                >
                                  PWD
                                </span>
                              )}
                              {resident.isSenior && (
                                <span
                                  className="badge"
                                  style={{
                                    fontSize: "0.7rem",
                                    background: "rgba(245, 158, 11, 0.1)",
                                    color: "var(--warning)",
                                  }}
                                >
                                  Senior
                                </span>
                              )}
                            </div>
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {isLoadingBHWResidents && (
            <div style={{ textAlign: "center", padding: "2rem", color: "var(--text-secondary)" }}>
              Loading residents...
            </div>
          )}

          {!isLoadingBHWResidents && bhwResidentsTotalCount > 0 && (
            <Pagination
              currentPage={reportPage}
              totalPages={bhwResidentsTotalPages}
              onPageChange={setReportPage}
              pageSize={reportPageSize}
              totalCount={bhwResidentsTotalCount}
            />
          )}
        </div>
      )}
    </div>
  );

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Barangay Health Workers (BHW)</h1>
          <p>Manage BHW profiles, assignments, visit logs, and trainings</p>
        </div>
        <button
          className="btn-primary"
          onClick={() => {
            setEditingItem(null);
            setShowModal(true);
          }}
        >
          <Plus size={20} />
          Add{" "}
          {tabs.find((t) => t.id === activeTab)?.label.split(" ")[0] || "Item"}
        </button>
      </div>

      <div className="tabs">
        {tabs.map((tab) => {
          const Icon = tab.icon;
          return (
            <button
              key={tab.id}
              className={`tab ${activeTab === tab.id ? "active" : ""}`}
              onClick={() => setActiveTab(tab.id)}
            >
              <Icon size={18} />
              {tab.label}
            </button>
          );
        })}
      </div>

      {activeTab === "profiles" && (
        <div className="search-bar">
          <Search size={20} />
          <input
            type="text"
            placeholder="Search BHW profiles..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      )}

      {activeTab === "profiles" && renderProfiles()}
      {activeTab === "assignments" && renderAssignments()}
      {activeTab === "visits" && renderVisits()}
      {activeTab === "trainings" && renderTrainings()}
      {activeTab === "deliveries" && renderDeliveries()}
      {activeTab === "kra-reports" && renderKRAReports()}
      {activeTab === "reports" && renderReports()}
      {activeTab === "profiles" && (
        <BHWProfileModal
          isOpen={showModal}
          onClose={() => {
            setShowModal(false);
            setEditingItem(null);
          }}
          bhwProfile={editingItem}
        />
      )}
      {activeTab === "assignments" && (
        <BHWAssignmentModal
          isOpen={showModal}
          onClose={() => {
            setShowModal(false);
            setEditingItem(null);
          }}
          assignment={editingItem}
        />
      )}
      {activeTab === "visits" && (
        <BHWVisitLogModal
          isOpen={showModal}
          onClose={() => {
            setShowModal(false);
            setEditingItem(null);
          }}
          visitLog={editingItem}
        />
      )}
      {activeTab === "trainings" && (
        <BHWTrainingModal
          isOpen={showModal}
          onClose={() => {
            setShowModal(false);
            setEditingItem(null);
          }}
          training={editingItem}
        />
      )}
      {activeTab === "deliveries" && (
        <DeliveryModal
          isOpen={showModal}
          onClose={() => {
            setShowModal(false);
            setEditingItem(null);
          }}
          delivery={editingItem}
        />
      )}
      {activeTab === "kra-reports" && (
        <KRAReportModal
          isOpen={showModal}
          onClose={() => {
            setShowModal(false);
            setEditingItem(null);
          }}
          report={editingItem}
        />
      )}
    </div>
  );
}
