namespace HospitalAppointment.Api.Models.Enums;

public enum ServiceStatus { Ordered, InProgress, Completed, Cancelled }
/*

Ordered	   Đã được chỉ định	Doctor chỉ định xét nghiệm máu.
InProgress	Đang thực hiện	Bệnh nhân đang được thực hiện xét nghiệm.
Completed	Đã hoàn thành	Xét nghiệm đã được thực hiện xong.
Cancelled	Đã hủy	Chỉ định dịch vụ bị hủy theo quy trình.
 */